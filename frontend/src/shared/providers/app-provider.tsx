"use client";

import { QueryClientProvider } from "@tanstack/react-query";
import { getQueryClient } from "../api/query-client";
import { Toaster } from "../components/ui/sonner";

export default function AppProvider({
  children,
}: {
  children: React.ReactNode;
}) {
  const queryClient = getQueryClient();
  return (
    <QueryClientProvider client={queryClient}>
      {children}
      <Toaster position="top-center" duration={3000} richColors closeButton />
    </QueryClientProvider>
  );
}
