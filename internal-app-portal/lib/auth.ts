import crypto from "node:crypto";

import { getPortalConfig } from "@/lib/env";

const SESSION_COOKIE_NAME = "portal_session";
const SESSION_TTL_SECONDS = 60 * 60 * 12;

type TokenKind = "session" | "download";

type TokenPayload = {
  exp: number;
  kind: TokenKind;
  sub: string;
};

type TokenValidationResult =
  | { ok: true; payload: TokenPayload }
  | { ok: false; status: 401 | 403; reason: string };

function base64UrlEncode(input: Buffer | string): string {
  return Buffer.from(input)
    .toString("base64")
    .replace(/\+/g, "-")
    .replace(/\//g, "_")
    .replace(/=+$/g, "");
}

function base64UrlDecode(input: string): Buffer {
  const normalized = input.replace(/-/g, "+").replace(/_/g, "/");
  const padding = normalized.length % 4 === 0 ? "" : "=".repeat(4 - (normalized.length % 4));
  return Buffer.from(normalized + padding, "base64");
}

function createSignature(payloadSegment: string): string {
  const { tokenSecret } = getPortalConfig();
  return base64UrlEncode(crypto.createHmac("sha256", tokenSecret).update(payloadSegment).digest());
}

function signPayload(payload: TokenPayload): string {
  const payloadSegment = base64UrlEncode(JSON.stringify(payload));
  const signature = createSignature(payloadSegment);
  return `${payloadSegment}.${signature}`;
}

function parseToken(token: string): TokenValidationResult {
  const [payloadSegment, signatureSegment] = token.split(".");

  if (!payloadSegment || !signatureSegment) {
    return { ok: false, status: 401, reason: "Malformed token" };
  }

  const expectedSignature = createSignature(payloadSegment);
  const provided = Buffer.from(signatureSegment);
  const expected = Buffer.from(expectedSignature);

  if (provided.length !== expected.length || !crypto.timingSafeEqual(provided, expected)) {
    return { ok: false, status: 403, reason: "Invalid token signature" };
  }

  try {
    const payload = JSON.parse(base64UrlDecode(payloadSegment).toString("utf8")) as TokenPayload;

    if (!payload.exp || !payload.kind || !payload.sub) {
      return { ok: false, status: 401, reason: "Incomplete token payload" };
    }

    if (payload.exp <= Math.floor(Date.now() / 1000)) {
      return { ok: false, status: 401, reason: "Expired token" };
    }

    return { ok: true, payload };
  } catch {
    return { ok: false, status: 401, reason: "Unreadable token payload" };
  }
}

function createToken(kind: TokenKind, sub: string, ttlSeconds: number): string {
  const payload: TokenPayload = {
    exp: Math.floor(Date.now() / 1000) + ttlSeconds,
    kind,
    sub
  };

  return signPayload(payload);
}

export function createSessionToken(): string {
  return createToken("session", "portal-user", SESSION_TTL_SECONDS);
}

export function createDownloadToken(): string {
  const { tokenTtlSeconds } = getPortalConfig();
  return createToken("download", "ota-download", tokenTtlSeconds);
}

export function validateSessionToken(token: string | undefined): TokenValidationResult {
  if (!token) {
    return { ok: false, status: 401, reason: "Missing session token" };
  }

  const result = parseToken(token);

  if (!result.ok) {
    return result;
  }

  if (result.payload.kind !== "session") {
    return { ok: false, status: 403, reason: "Unexpected token kind" };
  }

  return result;
}

export function validateDownloadToken(token: string | null): TokenValidationResult {
  if (!token) {
    return { ok: false, status: 401, reason: "Missing download token" };
  }

  const result = parseToken(token);

  if (!result.ok) {
    return result;
  }

  if (result.payload.kind !== "download") {
    return { ok: false, status: 403, reason: "Unexpected token kind" };
  }

  return result;
}

export function getSessionCookieName(): string {
  return SESSION_COOKIE_NAME;
}

export function getSessionTtlSeconds(): number {
  return SESSION_TTL_SECONDS;
}
