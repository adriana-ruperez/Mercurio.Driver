import { NextResponse } from "next/server";

import { getSessionCookieName } from "@/lib/auth";

export const dynamic = "force-dynamic";

const isProduction = process.env.NODE_ENV === "production";

export async function POST(request: Request) {
  const response = NextResponse.redirect(new URL("/", request.url), 303);
  response.cookies.set({
    name: getSessionCookieName(),
    value: "",
    httpOnly: true,
    maxAge: 0,
    path: "/",
    sameSite: "lax",
    secure: isProduction
  });

  return response;
}
