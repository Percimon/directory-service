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

export function unwrapEnvelope<T>(
  envelope: Envelope<T>,
  fallbackMessage: string,
): T {
  if (envelope.isError) {
    const message = envelope.error?.messages
      .map(({ message }) => message)
      .filter(Boolean)
      .join(", ");

    throw new Error(message || fallbackMessage);
  }

  if (envelope.result === null || envelope.result === undefined) {
    throw new Error("Invalid response format");
  }

  return envelope.result;
}
