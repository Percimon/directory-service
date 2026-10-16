import { z } from "zod";

export const locationFormSchema = z.object({
  name: z
    .string()
    .min(1, { message: "Name is required" })
    .max(100, { message: "Name must be at most 100 characters" }),
  city: z
    .string()
    .min(1, { message: "City is required" })
    .max(100, { message: "City must be at most 100 characters" }),
  district: z
    .string()
    .min(1, { message: "District is required" })
    .max(100, { message: "District must be at most 100 characters" }),
  street: z
    .string()
    .min(1, { message: "Street is required" })
    .max(100, { message: "Street must be at most 100 characters" }),
  structure: z
    .string()
    .min(1, { message: "Structure is required" })
    .max(100, { message: "Structure must be at most 100 characters" }),
  timeZone: z
    .string()
    .min(1, { message: "Time zone is required" })
    .max(100, { message: "Time zone must be at most 100 characters" }),
});

export type LocationFormData = z.infer<typeof locationFormSchema>;
