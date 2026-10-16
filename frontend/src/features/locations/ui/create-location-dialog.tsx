"use client";

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { LocationFormData, locationFormSchema } from "../model/schemas";
import { useCreateLocation } from "../model/use-create-location";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";

type CreateLocationFormProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

const defaultValues: LocationFormData = {
  name: "my-location",
  city: "Moscow",
  district: "Central",
  street: "Main Street",
  structure: "Building A",
  timeZone: "Europe/Moscow",
};

const fields = [
  { name: "name", label: "Название" },
  { name: "city", label: "Город" },
  { name: "district", label: "Район" },
  { name: "street", label: "Улица" },
  { name: "structure", label: "Строение" },
  { name: "timeZone", label: "Часовой пояс" },
] as const;

export default function CreateLocationDialog({
  open,
  onOpenChange,
}: CreateLocationFormProps) {
  const { createLocation, isPending } = useCreateLocation();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<LocationFormData>({
    defaultValues,
    resolver: zodResolver(locationFormSchema),
  });

  const handleOpenChange = (nextOpen: boolean) => {
    if (!nextOpen) {
      reset(defaultValues);
    }
    onOpenChange(nextOpen);
  };

  const onSubmit = async (data: LocationFormData) => {
    try {
      await createLocation({
        name: data.name,
        city: data.city,
        district: data.district,
        street: data.street,
        structure: data.structure,
        timeZone: data.timeZone,
      });
      handleOpenChange(false);
    } catch {
      // The mutation reports the request error with a toast.
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="max-h-[calc(100vh-2rem)] overflow-y-auto sm:max-w-xl">
        <DialogHeader>
          <DialogTitle>Создание локации</DialogTitle>
          <DialogDescription>
            Заполните форму для создания локации
          </DialogDescription>
        </DialogHeader>
        <form className="space-y-5" onSubmit={handleSubmit(onSubmit)} noValidate>
          <div className="grid gap-4 sm:grid-cols-2">
            {fields.map(({ name, label }) => {
              const error = errors[name];
              const errorId = `${name}-error`;

              return (
                <div className="space-y-2" key={name}>
                  <label
                    className="text-sm font-medium leading-none"
                    htmlFor={name}
                  >
                    {label}
                  </label>
                  <Input
                    id={name}
                    type="text"
                    aria-invalid={Boolean(error)}
                    aria-describedby={error ? errorId : undefined}
                    {...register(name)}
                  />
                  {error?.message && (
                    <p className="text-sm text-destructive" id={errorId}>
                      {error.message}
                    </p>
                  )}
                </div>
              );
            })}
          </div>
          <DialogFooter>
            <Button type="submit" disabled={isPending}>
              {isPending ? "Создание..." : "Создать"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
