import { locationsApi } from "@/entities/locations/api";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useCreateLocation() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: locationsApi.createLocation,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["locations"] });
      toast.success("Location created successfully");
    },
    onError: (error) => {
      toast.error(`Failed to create location: ${error}`);
    },
  });

  return {
    createLocation: mutation.mutateAsync,
    isPending: mutation.isPending,
  };
}
