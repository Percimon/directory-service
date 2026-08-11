"use client";

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
  SidebarTrigger,
  useSidebar,
} from "../../shared/components/ui/sidebar";
import { Home, Plus } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";

export default function AppSidebar() {
  const pathname = usePathname();

  const { setOpenMobile } = useSidebar();

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
    {
      href: routes.playground,
      label: "Playground",
      icon: Plus,
    },
  ];
  return (
    <Sidebar collapsible="icon">
      <SidebarHeader content="Меню">
        <SidebarTrigger />
      </SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>
              {menuItems.map((item) => {
                const isActive =
                  pathname === item.href ||
                  pathname.startsWith(item.href + "/");

                return (
                  <SidebarMenuItem key={item.href}>
                    <SidebarMenuButton
                      asChild
                      isActive={isActive}
                      tooltip={item.label}
                      className="hover:bg-gray-200 bg-accent data-[active=true]:bg-blue-200 transition-colors "
                      onClick={() => setOpenMobile(false)}
                    >
                      <Link
                        href={item.href}
                        className="flex items-center gap-3"
                      >
                        <item.icon className="h-5 w-5" />
                        <span>{item.label}</span>
                      </Link>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                );
              })}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
    </Sidebar>
  );
}
