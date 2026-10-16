"use client";

import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationNext,
  PaginationPrevious,
} from "@/shared/components/ui/pagination";
import { Spinner } from "@/shared/components/ui/spinner";
import { UseDepartmentsList } from "../model/use-departments-list";

export function DepartmentsList() {
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const { data, isPending, error } = UseDepartmentsList(page, pageSize);
  const departments = data?.items;
  const totalPages = data?.totalPages;
  const pageNumber = data?.page;

  if (isPending) {
    return <Spinner />;
  }

  if (error) {
    return (
      <div className="space-y-3">
        <p role="alert">Ошибка: {error.message}</p>
      </div>
    );
  }

  return (
    <section className="space-y-4">
      <h1 className="text-xl font-semibold">Подразделения</h1>
      {departments?.length === 0 ? (
        <p>Подразделения не найдены.</p>
      ) : (
        <ul className="space-y-2">
          {departments?.map((department) => (
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
      <Pagination aria-label="Навигация по страницам">
        <PaginationContent>
          <PaginationItem>
            <PaginationPrevious
              aria-label="Предыдущая страница"
              aria-disabled={page <= 1}
              className={
                page <= 1 ? "pointer-events-none opacity-50" : undefined
              }
              href="#"
              tabIndex={page <= 1 ? -1 : undefined}
              onClick={(event) => {
                event.preventDefault();
                if (page <= 1) return;
                setPage((currentPage) => Math.max(1, currentPage - 1));
              }}
            >
              Назад
            </PaginationPrevious>
          </PaginationItem>
          <PaginationItem>
            <span className="px-3 text-sm">
              Страница {pageNumber} из {totalPages}
            </span>
          </PaginationItem>
          <PaginationItem>
            <PaginationNext
              aria-label="Следующая страница"
              aria-disabled={page >= totalPages}
              className={
                page >= totalPages
                  ? "pointer-events-none opacity-50"
                  : undefined
              }
              href="#"
              tabIndex={page >= totalPages ? -1 : undefined}
              onClick={(event) => {
                event.preventDefault();
                if (page >= totalPages) return;
                setPage((currentPage) => Math.min(totalPages, currentPage + 1));
              }}
            >
              Далее
            </PaginationNext>
          </PaginationItem>
        </PaginationContent>
      </Pagination>
    </section>
  );
}
