"use client";

// import { Metadata } from "next";
import { LocationsList } from "../../features/locations/locations-pagination";
import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import CreateLocationDialog from "@/features/locations/create-location-dialog";

// export const metadata: Metadata = {
//   title: "Локации",
//   description: "Информация об локациях",
// };

export default function LocationsPage() {
  const [open, setOpen] = useState(false);

  return (
    <div>
      <Button onClick={() => setOpen(true)}>Создать локацию</Button>
      <CreateLocationDialog open={open} onOpenChange={setOpen} />
      <LocationsList />
    </div>
  );
}
