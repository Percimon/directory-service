import { locationsApi } from "@/entities/locations/api";
import { useQueryClient } from "@tanstack/react-query";

export default function useCreateLocation({
  name,
  city,
  district,
  street,
  structure,
  timeZone,
}: {
  name: string;
  city: string;
  district: string;
  street: string;
  structure: string;
  timeZone: string;
}) {
  const queryClient = useQueryClient();

  const [createLocation, isPending, error] = locationsApi.createLocation({
    name,
    city,
    district,
    street,
    structure,
    timeZone,
  });
  return;
}
