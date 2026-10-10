"use client";

import { useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import { departmentsApi } from "../../entities/departments/api";

export function DepartmentsList() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const {
    data: departments,
    isPending,
    isError,
    error,
  } = useQuery({
    queryKey: ["departments", { page, pageSize }],
    queryFn: ({ signal }) =>
      departmentsApi.getDepartments({ page, pageSize }, signal),
  });

  if (isPending) {
    return <Spinner />;
  }

  if (isError) {
    return (
      <div className="space-y-3">
        <p role="alert">Ошибка: {error.message}</p>
        <Button
          onClick={() => {
            queryClient.invalidateQueries({ queryKey: ["departments"] });
          }}
        >
          Попробовать снова
        </Button>
      </div>
    );
  }

  return (
    <section className="space-y-4">
      <h1 className="text-xl font-semibold">Подразделения</h1>
      {departments.items.length === 0 ? (
        <p>Подразделения не найдены.</p>
      ) : (
        <ul className="space-y-2">
          {departments.items.map((department) => (
            <li className="rounded-md border p-3" key={department.id}>
              <p className="font-medium">{department.name}</p>
              <p className="text-sm text-muted-foreground">{department.path}</p>
              <p className="text-sm text-muted-foreground">
                Slug: {department.slug}
              </p>
            </li>
          ))}
        </ul>
      )}
      <div className="flex items-center gap-3">
        <Button
          disabled={page <= 1}
          onClick={() => setPage((currentPage) => currentPage - 1)}
        >
          Назад
        </Button>
        <span>
          Страница {departments.pageNumber} из {departments.totalPages}
        </span>
        <Button
          disabled={page >= departments.totalPages}
          onClick={() => setPage((currentPage) => currentPage + 1)}
        >
          Далее
        </Button>
      </div>
    </section>
  );
}
