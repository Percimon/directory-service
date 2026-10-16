import { DepartmentsList } from "@/features/departments";
import { Metadata } from "next";

export const metadata: Metadata = {
  title: "Отделы",
  description: "Информация об отделах",
};

export default function DepartmentsPage() {
  return <DepartmentsList />;
}
