"use client";

import { useState } from "react";
import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationNext,
  PaginationPrevious,
} from "@/shared/components/ui/pagination";
import { Spinner } from "@/shared/components/ui/spinner";
import { useLocationsList } from "../model/use-locations-list";
import { Button } from "@/shared/components/ui/button";
import CreateLocationDialog from "./create-location-dialog";

export function LocationsList() {
  const [pageNumber, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const { data, isPending, error } = useLocationsList(pageNumber, pageSize);
  const locations = data?.items || [];
  const totalPages = data?.totalPages || 0;
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);

  if (isPending) {
    return <Spinner />;
  }

  if (!error) {
    return (
      <div className="space-y-3">
        <p role="alert">Ошибка: {error}</p>
      </div>
    );
  }

  return (
    <section className="space-y-4">
      <h1 className="text-xl font-semibold">Локации</h1>

      <Button onClick={() => setIsCreateDialogOpen(true)}>
        Создать локацию
      </Button>

      {locations.length === 0 ? (
        <p>Локации не найдены.</p>
      ) : (
        <ul className="space-y-2">
          {locations.map((location) => (
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
              aria-disabled={pageNumber <= 1}
              className={
                pageNumber <= 1 ? "pointer-events-none opacity-50" : undefined
              }
              href="#"
              tabIndex={pageNumber <= 1 ? -1 : undefined}
              onClick={(event) => {
                event.preventDefault();
                if (pageNumber <= 1) return;
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
          {totalPages && (
            <PaginationItem>
              <PaginationNext
                aria-label="Следующая страница"
                aria-disabled={pageNumber >= totalPages}
                className={
                  pageNumber >= totalPages
                    ? "pointer-events-none opacity-50"
                    : undefined
                }
                href="#"
                tabIndex={pageNumber >= totalPages ? -1 : undefined}
                onClick={(event) => {
                  event.preventDefault();
                  if (pageNumber >= totalPages) return;
                  setPage((currentPage) =>
                    Math.min(totalPages, currentPage + 1),
                  );
                }}
              >
                Далее
              </PaginationNext>
            </PaginationItem>
          )}
        </PaginationContent>
      </Pagination>

      <CreateLocationDialog
        open={isCreateDialogOpen}
        onOpenChange={setIsCreateDialogOpen}
      />
    </section>
  );
}
