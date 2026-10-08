import { apiClient } from "@/shared/api/axios-instance";
import { unwrapEnvelope } from "@/shared/api/envelope";
import type { Envelope } from "@/shared/api/envelope";
import type { Department } from "./types";

export type GetDepartmentsRequest = {
  page: number;
  pageSize: number;
  search?: string;
};

export type GetDepartmentsResponse = {
  items: Department[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
};

export const departmentsApi = {
  getDepartments: async (
    request: GetDepartmentsRequest,
    signal?: AbortSignal,
  ): Promise<GetDepartmentsResponse> => {
    const response = await apiClient.get<Envelope<GetDepartmentsResponse>>(
      "/departments",
      {
        params: request,
        signal,
      },
    );

    const result = unwrapEnvelope(
      response.data,
      "Не удалось загрузить подразделения",
    );
    if (
      !Array.isArray(result.items) ||
      typeof result.totalCount !== "number" ||
      typeof result.pageNumber !== "number" ||
      typeof result.pageSize !== "number" ||
      typeof result.totalPages !== "number"
    ) {
      throw new Error("Сервер вернул некорректный список подразделений");
    }

    return result;
  },
};
