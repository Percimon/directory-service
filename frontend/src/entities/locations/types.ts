export type Location = {
  id: string;
  name: string;
  city: string;
  district: string;
  street: string;
  structure: string;
  createdAt: string;
  departmentsCount: number;
};

export type GetLocationsParams = {
  search?: string;
  pageNumber: number;
  pageSize: number;
};

export type GetLocationsResponse = {
  items: Location[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
};

export type CreateLocationRequest = {
  name: string;
  city: string;
  district: string;
  street: string;
  structure: string;
  timeZone: string;
};
