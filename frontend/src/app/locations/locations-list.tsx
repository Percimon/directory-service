"use client";

import { useEffect, useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import { locationsApi } from "./api";
import { Location } from "./types";

export function LocationsList() {
  const [locations, setLocations] = useState<Location[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [retryCount, setRetryCount] = useState(0);

  useEffect(() => {
    let isActive = true;

    locationsApi
      .getLocations({ page: 1, pageSize: 10 })
      .then((data) => {
        if (isActive) {
          setLocations(data);
        }
      })
      .catch((requestError: unknown) => {
        if (isActive) {
          setError(
            requestError instanceof Error
              ? requestError.message
              : "Не удалось загрузить локации",
          );
        }
      })
      .finally(() => {
        if (isActive) {
          setLoading(false);
        }
      });

    return () => {
      isActive = false;
    };
  }, [retryCount]);

  const retry = () => {
    setError(null);
    setLoading(true);
    setRetryCount((count) => count + 1);
  };

  if (loading) {
    return <Spinner />;
  }

  if (error) {
    return (
      <div className="space-y-3">
        <p role="alert">Ошибка: {error}</p>
        <Button onClick={retry}>Повторить</Button>
      </div>
    );
  }

  return (
    <section className="space-y-4">
      <h1 className="text-xl font-semibold">Локации</h1>
      {locations.length === 0 ? (
        <p>Локации не найдены.</p>
      ) : (
        <ul className="space-y-2">
          {locations.map((location) => (
            <li className="rounded-md border p-3" key={location.id}>
              <p className="font-medium">{location.name}</p>
              <p className="text-sm text-muted-foreground">
                {[location.city, location.district, location.street, location.structure]
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
    </section>
  );
}
