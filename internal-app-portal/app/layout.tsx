import type { Metadata } from "next";

import { getPortalConfig } from "@/lib/env";

import "./globals.css";

export const metadata: Metadata = {
  title: "Mercurio Driver Internal Portal",
  description: "Portal interno para distribucion OTA de Mercurio Driver."
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  const { appTitle } = getPortalConfig();

  return (
    <html lang="es">
      <body data-app-title={appTitle}>{children}</body>
    </html>
  );
}
