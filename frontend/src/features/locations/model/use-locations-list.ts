import { locationsQueryOptions } from "@/entities/locations/api";
import { useQuery } from "@tanstack/react-query";

export function useLocationsList(page: number, pageSize: number) {
  const { data, isPending, error } = useQuery(
    locationsQueryOptions.getLocationsOptions(page, pageSize),
  );

  return { data, isPending, error };
}
