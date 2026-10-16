"use client";

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
} from "@/shared/components/ui/dialog";
import { LocationFormData, locationFormSchema } from "../model/schemas";
import { useCreateLocation } from "../model/use-create-location";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";

type CreateLocationFormProps = {
  open: boolean;
  onOpenChange: (open: true) => void;
};

const defaultValues: LocationFormData = {
  name: "my-location",
  city: "Moscow",
  district: "Central",
  street: "Main Street",
  structure: "Building A",
  timeZone: "Europe/Moscow",
};

export default function CreateLocationDialog({
  open,
  onOpenChange,
}: CreateLocationFormProps) {
  const { createLocation } = useCreateLocation();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<LocationFormData>({
    defaultValues,
    resolver: zodResolver(locationFormSchema),
  });

  const handleClose = () => {
    reset(defaultValues);
    onOpenChange(false);
  };

  const onSubmit = (data: LocationFormData) => {
    handleClose();
    void (async () => {
      await createLocation({
        name: data.name,
        city: data.city,
        district: data.district,
        street: data.street,
        structure: data.structure,
        timeZone: data.timeZone,
      }).catch(() => undefined);
    })();
  };

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent>
          <DialogHeader>Создание локации</DialogHeader>
          <DialogDescription>
            Заполните форму для создания локации
          </DialogDescription>
          <form className="grid space-y-4" onSubmit={handleSubmit(onSubmit)}>
            <div>
              <label htmlFor="name">Название</label>
              <input id="name" type="text" {...register("name")} />
            </div>
            <div>
              <label htmlFor="city">Город</label>
              <input id="city" type="text" {...register("city")} />
            </div>
            <div>
              <label htmlFor="district">Район</label>
              <input id="district" type="text" {...register("district")} />
            </div>
            <div>
              <label htmlFor="street">Улица</label>
              <input id="street" type="text" {...register("street")} />
            </div>
            <div>
              <label htmlFor="structure">Строение</label>
              <input id="structure" type="text" {...register("structure")} />
            </div>
            <div>
              <label htmlFor="timeZone">Часовой пояс</label>
              <input id="timeZone" type="text" {...register("timeZone")} />
            </div>
            <button type="submit">Создать</button>
          </form>
        </DialogContent>
      </Dialog>
    </>
  );
}
