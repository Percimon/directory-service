import { Metadata } from "next";
import { LocationsList } from "./locations-list";

export const metadata: Metadata = {
  title: "Локации",
  description: "Информация об локациях",
};

export default function LocationsPage() {
  return <LocationsList />;
}
