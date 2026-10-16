import { departmentsApi } from "@/entities/departments";
import { useQuery } from "@tanstack/react-query";

export function UseDepartmentsList(page: number, pageSize: number) {
  const { data, isPending, error } = useQuery({
    queryKey: ["departments", { page, pageSize }],
    queryFn: () => departmentsApi.getDepartments({ page, pageSize }),
    select: (data) => data.result,
  });

  return {
    data,
    isPending,
    error,
  };
}
