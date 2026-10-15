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

    return res.data;
  },

  createLocation: async (
    request: CreateLocationRequest,
  ): Promise<Envelope<string>> => {
    const response = await apiClient.post<Envelope<string>>(
      "/locations",
      request,
    );

    return response.data;
  },
};

export const locationsQueryOptions = {
  baseKey: "lessons",

  getLocationsOptions: (page: number, pageSize: number) => {
    return queryOptions({
      queryKey: [locationsQueryOptions.baseKey, { pageNumber: page, pageSize }],
      queryFn: () =>
        locationsApi.getLocations({ page: page, pageSize: pageSize }),
      select: (data) => data.result,
    });
  },
};
