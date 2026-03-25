import { NextResponse } from "next/server";

import { createSessionToken, getSessionCookieName, getSessionTtlSeconds } from "@/lib/auth";
import { getPortalConfig } from "@/lib/env";

export const dynamic = "force-dynamic";

const isProduction = process.env.NODE_ENV === "production";

export async function POST(request: Request) {
  const formData = await request.formData();
  const username = String(formData.get("username") ?? "").trim();
  const password = String(formData.get("password") ?? "");

  if (!username || !password) {
    return NextResponse.redirect(new URL("/?error=missing_credentials", request.url), 303);
  }

  const config = getPortalConfig();

  if (username !== config.username || password !== config.password) {
    return NextResponse.redirect(new URL("/?error=invalid_credentials", request.url), 303);
  }

  const response = NextResponse.redirect(new URL("/download", request.url), 303);
  response.cookies.set({
    name: getSessionCookieName(),
    value: createSessionToken(),
    httpOnly: true,
    maxAge: getSessionTtlSeconds(),
    path: "/",
    sameSite: "lax",
    secure: isProduction
  });

  return response;
}
