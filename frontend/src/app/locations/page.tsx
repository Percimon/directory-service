import { LocationsList } from "@/features/locations";
import { Metadata } from "next";

export const metadata: Metadata = {
  title: "Локации",
  description: "Информация об локациях",
};

export default function LocationsPage() {
  return (
    <div>
      <LocationsList />
    </div>
  );
}
