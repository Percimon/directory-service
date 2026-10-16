import { apiClient } from "@/shared/api/axios-instance";
import type { Envelope } from "@/shared/api/envelope";
import { GetDepartmentsParams, GetDepartmentsResponse } from "./types";

export const departmentsApi = {
  getDepartments: async (
    params: GetDepartmentsParams,
  ): Promise<Envelope<GetDepartmentsResponse>> => {
    const res = await apiClient.get<Envelope<GetDepartmentsResponse>>(
      "/departments",
      {
        params: params,
      },
    );

    return res.data;
  },
};
