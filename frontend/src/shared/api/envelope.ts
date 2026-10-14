export type Envelope<T = unknown> = {
  result: T | null;
  error: ApiError | null;
  isError: boolean;
  timeGenerated: string;
};

type ApiError = {
  messages: ErrorMessage[];
  type: ErrorType;
};

type ErrorMessage = {
  code: string;
  message: string;
  invalidField?: string | null;
};

type ErrorType =
  | "validation"
  | "not_found"
  | "failure"
  | "conflict"
  | "authorization"
  | "authentication";
