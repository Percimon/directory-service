import { departmentsApi } from "@/entities/departments";
import { useQuery, useQueryClient } from "@tanstack/react-query";

export function UseDepartmentsList(page: number, pageSize: number) {
  const queryClient = useQueryClient();
  const { data, isPending, error } = useQuery({
    queryKey: ["departments", { page, pageSize }],
    queryFn: ({ signal }) =>
      departmentsApi.getDepartments({ page, pageSize }, signal),
  });

  return {
    data,
    isPending,
    error,
  };
}
