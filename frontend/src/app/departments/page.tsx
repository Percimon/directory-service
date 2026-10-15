import { Metadata } from "next";
import { DepartmentsList } from "../../features/departments/ui/departments-list";

export const metadata: Metadata = {
  title: "Отделы",
  description: "Информация об отделах",
};

export default function DepartmentsPage() {
  return <DepartmentsList />;
}
