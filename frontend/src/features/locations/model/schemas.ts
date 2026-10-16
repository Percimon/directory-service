import { z } from "zod";

export const locationFormSchema = z.object({
  name: z
    .string()
    .min(1, { message: "Укажите название" })
    .max(100, { message: "Название не должно превышать 100 символов" }),
  city: z
    .string()
    .min(1, { message: "Укажите город" })
    .max(100, { message: "Город не должен превышать 100 символов" }),
  district: z
    .string()
    .min(1, { message: "Укажите район" })
    .max(100, { message: "Район не должен превышать 100 символов" }),
  street: z
    .string()
    .min(1, { message: "Укажите улицу" })
    .max(100, { message: "Улица не должна превышать 100 символов" }),
  structure: z
    .string()
    .min(1, { message: "Укажите строение" })
    .max(100, { message: "Строение не должно превышать 100 символов" }),
  timeZone: z
    .string()
    .min(1, { message: "Укажите часовой пояс" })
    .max(100, { message: "Часовой пояс не должен превышать 100 символов" }),
});

export type LocationFormData = z.infer<typeof locationFormSchema>;
