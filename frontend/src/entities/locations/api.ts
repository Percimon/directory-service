import { apiClient } from "@/shared/api/axios-instance";
import type { Envelope } from "@/shared/api/envelope";
import type {
  CreateLocationRequest,
  GetLocationsParams,
  GetLocationsResponse,
} from "./types";
import { queryOptions } from "@tanstack/react-query";

export const locationsApi = {
  getLocations: async (
    params: GetLocationsParams,
  ): Promise<Envelope<GetLocationsResponse>> => {
    const res = await apiClient.get<Envelope<GetLocationsResponse>>(
      "/locations",
      {
        params: params,
      },
    );

    if (res.data.isError) {
      const message =
        res.data.error?.messages?.map((m) => m.message).join(", ") ||
        "Не удалось получить локации";
      throw new Error(message);
    }

    return res.data;
  },

  createLocation: async (
    request: CreateLocationRequest,
  ): Promise<Envelope<string>> => {
    const response = await apiClient.post<Envelope<string>>(
      "/locations",
      request,
    );

    if (response.data.isError) {
      const message =
        response.data.error?.messages?.map((m) => m.message).join(", ") ||
        "Не удалось создать локацию";
      throw new Error(message);
    }

    return response.data;
  },
};

export const locationsQueryOptions = {
  baseKey: "locations",

  getLocationsOptions: (page: number, pageSize: number) => {
    return queryOptions({
      queryKey: [locationsQueryOptions.baseKey, { pageNumber: page, pageSize }],
      queryFn: () =>
        locationsApi.getLocations({ page: page, pageSize: pageSize }),
      select: (data) => data.result,
    });
  },
};
