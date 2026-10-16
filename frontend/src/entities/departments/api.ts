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

    if (res.data.isError) {
      const message =
        res.data.error?.messages?.map((m) => m.message).join(", ") ||
        "Не удалось получить отделы";
      throw new Error(message);
    }

    return res.data;
  },
};
