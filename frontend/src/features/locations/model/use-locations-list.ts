import { locationsQueryOptions } from "@/entities/locations/api";
import { useQuery } from "@tanstack/react-query";

export function useLocationsList({
  pageNumber,
  pageSize,
}: {
  pageNumber: number;
  pageSize: number;
}) {
  const { data, isPending, error } = useQuery(
    locationsQueryOptions.getLocationsOptions({ pageNumber, pageSize }),
  );

  return {
    locations: data?.items,
    totalPages: data?.totalPages,
    totalCount: data?.totalCount,
    isPending,
    error,
  };
}
