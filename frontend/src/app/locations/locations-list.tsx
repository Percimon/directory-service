"use client";

import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
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
      <div className="flex items-center gap-3">
        <Button
          disabled={page <= 1}
          onClick={() => setPage((currentPage) => currentPage - 1)}
        >
          Назад
        </Button>
        <span>
          Страница {locations.pageNumber} из {locations.totalPages}
        </span>
        <Button
          disabled={page >= locations.totalPages}
          onClick={() => setPage((currentPage) => currentPage + 1)}
        >
          Далее
        </Button>
      </div>
    </section>
  );
}
