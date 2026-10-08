type PagedList<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
};
