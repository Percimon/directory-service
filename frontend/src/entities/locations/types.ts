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
  page: number;
  pageSize: number;
};

export type GetLocationsResponse = {
  items: Location[];
  totalCount: number;
  page: number;
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
