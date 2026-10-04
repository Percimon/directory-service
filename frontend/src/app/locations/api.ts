import { apiClient } from "@/shared/api/axios-instance";
import { Location } from "./types";

export type GetLocationsRequest = {
  search?: string;
  page: number;
  pageSize: number;
};

type PagedList<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
};

export const locationsApi = {
  getLocations: async (request: GetLocationsRequest): Promise<Location[]> => {
    const response = await apiClient.get<Envelope<PagedList<Location>>>(
      "/locations",
      {
        params: request,
      },
    );

    if (response.data.isError) {
      const message = response.data.error?.messages
        .map(({ message }) => message)
        .filter(Boolean)
        .join(", ");

      throw new Error(message || "Не удалось загрузить локации");
    }

    const locations = response.data.result?.items;
    if (!Array.isArray(locations)) {
      throw new Error("Сервер вернул некорректный список локаций");
    }

    return locations;
  },
};

export type Envelope<T = unknown> = {
  result: T | null;
  error: ApiError | null;
  isError: boolean;
  timeGenerated: string;
};

export type ApiError = {
  messages: ErrorMessage[];
  type: ErrorType;
};

export type ErrorMessage = {
  code: string;
  message: string;
  invalidField?: string | null;
};

export type ErrorType =
  | "validation"
  | "not_found"
  | "failure"
  | "conflict"
  | "authorization"
  | "authentication";
