"use client";

// import { Metadata } from "next";
import { LocationsList } from "../../features/locations/locations-pagination";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
} from "@/shared/components/ui/dialog";
import { useState } from "react";
import { Button } from "@/shared/components/ui/button";

// export const metadata: Metadata = {
//   title: "Локации",
//   description: "Информация об локациях",
// };

export default function LocationsPage() {
  const [open, setOpen] = useState(false);

  return (
    <div>
      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>Создание локации</DialogHeader>
          <DialogDescription>
            Заполните форму для создания локации
          </DialogDescription>
        </DialogContent>
      </Dialog>

      <Button onClick={() => setOpen(true)}>Создать локацию</Button>

      <LocationsList />
    </div>
  );
}
