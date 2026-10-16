export type Department = {
  id: string;
  name: string;
  slug: string;
  path: string;
  createdAt: string;
};

export type GetDepartmentsParams = {
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
