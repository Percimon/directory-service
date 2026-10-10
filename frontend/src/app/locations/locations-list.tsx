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
import { locationsApi } from "../../entities/locations/api";
import { useQuery, useQueryClient } from "@tanstack/react-query";

export function LocationsList() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const {
    data: locations,
    isPending,
    isError,
    error,
  } = useQuery({
    queryKey: ["locations", { page, pageSize }],
    queryFn: ({ signal }) =>
      locationsApi.getLocations({ page, pageSize }, signal),
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
            queryClient.invalidateQueries({ queryKey: ["locations"] });
          }}
        >
          Попробовать снова
        </Button>
      </div>
    );
  }

  return (
    <section className="space-y-4">
      <h1 className="text-xl font-semibold">Локации</h1>
      {locations.items.length === 0 ? (
        <p>Локации не найдены.</p>
      ) : (
        <ul className="space-y-2">
          {locations.items.map((location) => (
            <li className="rounded-md border p-3" key={location.id}>
              <p className="font-medium">{location.name}</p>
              <p className="text-sm text-muted-foreground">
                {[
                  location.city,
                  location.district,
                  location.street,
                  location.structure,
                ]
                  .filter(Boolean)
                  .join(", ")}
              </p>
              <p className="text-sm text-muted-foreground">
                Подразделений: {location.departmentsCount}
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
              className={page <= 1 ? "pointer-events-none opacity-50" : undefined}
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
              Страница {locations.pageNumber} из {locations.totalPages}
            </span>
          </PaginationItem>
          <PaginationItem>
            <PaginationNext
              aria-label="Следующая страница"
              aria-disabled={page >= locations.totalPages}
              className={
                page >= locations.totalPages
                  ? "pointer-events-none opacity-50"
                  : undefined
              }
              href="#"
              tabIndex={page >= locations.totalPages ? -1 : undefined}
              onClick={(event) => {
                event.preventDefault();
                if (page >= locations.totalPages) return;
                setPage((currentPage) =>
                  Math.min(locations.totalPages, currentPage + 1),
                );
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
