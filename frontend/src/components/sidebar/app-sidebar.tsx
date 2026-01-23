import { routes } from "@/shared/routes";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
} from "../ui/sidebar";
import { Home, Plus } from "lucide-react";
import Link from "next/link";

export default function AppSidebar() {
  const menuItems = [
    {
      href: routes.home,
      label: "Главная",
      icon: Home,
    },
    {
      href: routes.departments,
      label: "Отделы",
      icon: Plus,
    },
    {
      href: routes.locations,
      label: "Локации",
      icon: Plus,
    },
    {
      href: routes.positions,
      label: "Позиции",
      icon: Plus,
    },
  ];
  return (
    <Sidebar>
      <SidebarHeader>Меню</SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>
              {menuItems.map((item) => (
                <SidebarMenuItem key={item.href}>
                  <SidebarMenuButton
                    asChild
                    className="bg-accent transition-colors"
                  >
                    <Link href={item.href} className="flex items-center gap-3">
                      <item.icon className="h-5 w-5" />
                      <span>{item.label}</span>
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
    </Sidebar>
  );
}
