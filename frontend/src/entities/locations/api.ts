import { apiClient } from "@/shared/api/axios-instance";
import { unwrapEnvelope } from "@/shared/api/envelope";
import type { Envelope } from "@/shared/api/envelope";
import type { Location } from "./types";

export type GetLocationsRequest = {
  search?: string;
  page: number;
  pageSize: number;
};

export type GetLocationsResponse = {
  items: Location[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
};

export const locationsApi = {
  getLocations: async (
    request: GetLocationsRequest,
    signal?: AbortSignal,
  ): Promise<GetLocationsResponse> => {
    const response = await apiClient.get<Envelope<GetLocationsResponse>>(
      "/locations",
      {
        params: request,
      },
    );

    const result = unwrapEnvelope(
      response.data,
      "Не удалось загрузить локации",
    );
    if (
      !Array.isArray(result.items) ||
      typeof result.totalCount !== "number" ||
      typeof result.pageNumber !== "number" ||
      typeof result.pageSize !== "number" ||
      typeof result.totalPages !== "number"
    ) {
      throw new Error("Сервер вернул некорректный список локаций");
    }

    return result;
  },
};
