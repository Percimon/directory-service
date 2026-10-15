import { apiClient } from "@/shared/api/axios-instance";
import type { Envelope } from "@/shared/api/envelope";
import { GetDepartmentsParams, GetDepartmentsResponse } from "./types";

export const departmentsApi = {
  getDepartments: async (
    request: GetDepartmentsParams,
  ): Promise<Envelope<GetDepartmentsResponse>> => {
    const response = await apiClient.get<Envelope<GetDepartmentsResponse>>(
      "/departments",
      {
        params: request,
      },
    );
    return response.data;
  },
};
