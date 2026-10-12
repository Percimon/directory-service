import { Metadata } from "next";
import { DepartmentsList } from "../../features/departments/departments-pagination";

export const metadata: Metadata = {
  title: "Отделы",
  description: "Информация об отделах",
};

export default function DepartmentsPage() {
  return <DepartmentsList />;
}
