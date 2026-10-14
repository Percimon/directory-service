"use client";

import { SidebarProvider } from "@/shared/components/ui/sidebar";
import AppSidebar from "../sidebar/app-sidebar";
import Header from "../header/header";
import { Toaster } from "@/shared/components/ui/sonner";
import AppProvider from "@/shared/providers/app-provider";

export default function AppLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <AppProvider>
      <SidebarProvider>
        <div className="flex h-screen w-full">
          <AppSidebar />
          <div className="flex-1 flex flex-col min-w-0">
            <Header />
            <main className="flex-1 overflow-auto p-10">{children}</main>
            <Toaster className="top-center" duration={3000} richColors={true} />
          </div>
        </div>
      </SidebarProvider>
    </AppProvider>
  );
}
