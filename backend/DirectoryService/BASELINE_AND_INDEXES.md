# Базовый план и минимальные индексы для read-запросов отделов

## Краткая карта: какой индекс ускоряет какой endpoint

- `idx_departments_path` (`GIST path`) — ускоряет `GET /departments/{id}/ancestors` и любые запросы по предкам / subtree / ltree-предикатам.
- `IX_departments_fk_parent_id` / `idx_departments_fk_parent_id` (`BTREE fk_parent_id`) — ускоряют `GET /departments/tree` и `GET /departments/{id}/children`.
- `idx_departments_name_lower_trgm` (`GIN (LOWER(name))`) — предназначен для `GET /departments/tree/search?q=...`, но при `ILIKE '%...%'` с ведущим `%` планировщик всё равно часто выбирает `Seq Scan`.
- `IX_department_locations_department_id` / `IX_department_locations_location_id` — ускоряют joins и выборки по локациям.
- `IX_department_positions_department_id` / `IX_department_positions_position_id` — ускоряют joins и выборки по должностям.

## Baseline до оптимизации

Датасет: 10 000 отделов, созданных через seed endpoint.
БД: PostgreSQL 16 + расширение `ltree`.

| Запрос | Краткое описание плана | Фактическое время |
|---|---|---:|
| `GET /departments/tree` | `Bitmap Heap Scan` по `departments` с использованием `IX_departments_fk_parent_id`, фильтр `fk_parent_id IS NULL`, затем сортировка по `name` | `2.353 ms` |
| `GET /departments/{id}/children` | `Index Scan using "IX_departments_fk_parent_id"` с фильтром `department.is_active` и сортировкой по `name` | `9.540 ms` |
| `GET /departments/{id}/ancestors` | `Bitmap Index Scan on idx_departments_path`, фильтр `path @> ...` и `path <> ...` | `12.709 ms` |
| `GET /departments/tree/search?q=seed` | `Seq Scan` по `departments`, фильтр `name ILIKE '%seed%'`, сортировка по `path` с external merge | `312.802 ms` |

### Комментарии по captured `EXPLAIN (ANALYZE, BUFFERS)`

- root и children уже использовали существующий индекс по `fk_parent_id`.
- ancestors использовал GIST-индекс по `ltree path` и оставался быстрым на текущем объёме данных.
- самый проблемный сценарий — поиск по имени: условие `ILIKE '%seed%'` приводило к full scan и становилось самым дорогим запросом.

## Минимальный набор индексов, который был добавлен

Минимальный набор индексов, который был добавлен для известных сценариев:

- `departments.path` — `GIST` для ancestors / sub-tree / `ltree`-предикатов
- `departments.fk_parent_id` — `BTREE` для root-level и прямых детей
- `departments.name` — trigram-index для поиска без учёта регистра (`pg_trgm` + `LOWER(name)`)
- `department_locations.location_id` — `BTREE` для lookups по локациям и join'ов
- `department_locations.department_id` — `BTREE` для получения списка локаций подразделения
- `department_positions.department_id` — `BTREE` для join-сценариев по подразделениям
- `department_positions.position_id` — `BTREE` для обратных joins по должностям

Реализация находится в конфигурациях EF, а отдельная миграция служит страховкой для extension `pg_trgm` и индекса поиска.

## После оптимизации

Тот же набор из 10 000 отделов использовался, и те же запросы были выполнены через `EXPLAIN (ANALYZE, BUFFERS)` после добавления индексов.

| Запрос | Краткое описание плана после индексов | Время после |
|---|---|---:|
| `GET /departments/tree` | `Bitmap Heap Scan` -> `Bitmap Index Scan on idx_departments_fk_parent_id`, затем сортировка по `name` | `1.754 ms` |
| `GET /departments/{id}/children` | `Index Scan using idx_departments_fk_parent_id` с фильтром `department.is_active` | `6.068 ms` |
| `GET /departments/{id}/ancestors` | `Bitmap Index Scan on idx_departments_path` с `path @> ...` | `12.398 ms` |
| `GET /departments/tree/search?q=seed` | всё ещё используется `Seq Scan`; фильтр `department.name ~~* '%seed%'` | `177.570 ms` |

### Почему поиск по имени всё ещё не использует индекс

Поиск по имени всё ещё выполняется через `Seq Scan` даже после добавления trigram-индекса по причине:

- условие `ILIKE '%seed%'` содержит ведущий wildcard;
- это не селективный поиск, а поиск по вхождению в строке;
- планировщик видит, что почти вся таблица подходит под условие (`rows ~ 20000` из `20009`), поэтому full scan дешевле, чем index walk;
- `gin_trgm` на `LOWER(name)` не делает leading-wildcard-поиск выгодным при таком характере данных.

Иными словами, индекс правильный по типу, но не подходит под этот конкретный паттерн запроса. Более удачный дизайн поиска:

- нормализованное поле/токенизация;
- поиск по префиксу (`LIKE 'seed%'`) для автодополнения;
- или отдельный trigram-search с паттерном, индексируемым эффективнее, чем `'%...%'`.

## Оценка partial index (`is_active = true`)

Мы отдельно проверяли идею partial index только для активных строк. В этом наборе данных распределение практически такое:

- всего строк: `20009`
- активных: `20008`
- удалённых / inactive: `1`

То есть селективность partial index очень низкая, и общий выигрыш минимален.

### Экспериментальные проверки

- `idx_departments_active_parent_id` на `(fk_parent_id) WHERE is_active = true`
  - root query: `0.117 ms` vs baseline `1.754 ms`
  - но обычный индекс уже даёт почти тот же план; разница слишком мала, чтобы оправдать дополнительный индекс
- `idx_departments_active_path` на `path WHERE is_active = true`
  - ancestors query: `12.215 ms` vs baseline `12.398 ms`
  - практически никакого выигрыша
- `idx_departments_active_name_lower_trgm` на `LOWER(name) WHERE is_active = true`
  - для `ILIKE '%seed%'` всё равно `Seq Scan`
  - реального выигрыша нет

### Решение

Partial index по active-строкам добавлять не стали. Обычные индексы уже покрывают hot read-пути достаточно хорошо, а partial-варианты либо:

- дают лишь минимальный выигрыш, не оправдывающий дополнительное обслуживание;
- либо вообще не меняют фактический план.

Таким образом, финальный набор индексов остаётся обычный, без partial-index-магии.

## SQL, использованный для baseline capture

```sql
EXPLAIN (ANALYZE, BUFFERS, VERBOSE, FORMAT TEXT)
SELECT
    department.id AS Id,
    department.fk_parent_id AS ParentId,
    department.name AS Name,
    department.slug AS Slug,
    department.path::text AS Path,
    department.depth AS Depth,
    EXISTS (
        SELECT 1
        FROM departments child
        WHERE child.fk_parent_id = department.id
          AND child.is_active = true) AS HasChildren,
    (
        SELECT COUNT(*)::int
        FROM departments child
        WHERE child.fk_parent_id = department.id
          AND child.is_active = true) AS ChildrenCount
FROM departments department
WHERE department.is_active = true
  AND department.fk_parent_id IS NULL
ORDER BY department.name;
```

```sql
EXPLAIN (ANALYZE, BUFFERS, VERBOSE, FORMAT TEXT)
SELECT
    department.id AS Id,
    department.fk_parent_id AS ParentId,
    department.name AS Name,
    department.slug AS Slug,
    department.path::text AS Path,
    department.depth AS Depth,
    EXISTS (
        SELECT 1
        FROM departments child
        WHERE child.fk_parent_id = department.id
          AND child.is_active = true) AS HasChildren,
    (
        SELECT COUNT(*)::int
        FROM departments child
        WHERE child.fk_parent_id = department.id
          AND child.is_active = true) AS ChildrenCount
FROM departments department
WHERE department.is_active = true
  AND department.name ILIKE '%seed%'
ORDER BY department.path;
```

```sql
EXPLAIN (ANALYZE, BUFFERS, VERBOSE, FORMAT TEXT)
SELECT
    department.id AS Id,
    department.fk_parent_id AS ParentId,
    department.name AS Name,
    department.slug AS Slug,
    department.path::text AS Path,
    department.depth AS Depth,
    EXISTS (
        SELECT 1
        FROM departments child
        WHERE child.fk_parent_id = department.id
          AND child.is_active = true) AS HasChildren,
    (
        SELECT COUNT(*)::int
        FROM departments child
        WHERE child.fk_parent_id = department.id
          AND child.is_active = true) AS ChildrenCount
FROM departments department
WHERE department.is_active = true
  AND department.fk_parent_id IS NULL
ORDER BY department.name;
```

```sql
EXPLAIN (ANALYZE, BUFFERS, VERBOSE, FORMAT TEXT)
SELECT
    department.id AS Id,
    department.fk_parent_id AS ParentId,
    department.name AS Name,
    department.slug AS Slug,
    department.path::text AS Path,
    department.depth AS Depth,
    EXISTS (
        SELECT 1
        FROM departments child
        WHERE child.fk_parent_id = department.id
          AND child.is_active = true) AS HasChildren,
    (
        SELECT COUNT(*)::int
        FROM departments child
        WHERE child.fk_parent_id = department.id
          AND child.is_active = true) AS ChildrenCount
FROM departments department
WHERE department.is_active = true
  AND department.name ILIKE '%seed%'
ORDER BY department.path;
```
