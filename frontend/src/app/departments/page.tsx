import { Metadata } from "next";
import { DepartmentsList } from "./departments-list";

export const metadata: Metadata = {
  title: "Отделы",
  description: "Информация об отделах",
};

export default function DepartmentsPage() {
  return <DepartmentsList />;
}
