import { apiClient, Envelope } from "@/shared/api/axios-instance";

export type GetLocationsRequest = {
  search?: string;
  page: number;
  pageSize: number;
};

export const locationsApi = {
  getLocations: async (request: GetLocationsRequest): Promise<Location[]> => {
    const response = await apiClient.get<Envelope<Location[]>>("/locations", {
      params: request,
    });
    return response.data || [];
  },
};
