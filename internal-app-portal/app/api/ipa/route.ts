import { access, readFile, stat } from "node:fs/promises";
import path from "node:path";

import { NextResponse } from "next/server";

import { validateDownloadToken } from "@/lib/auth";

export const dynamic = "force-dynamic";
export const runtime = "nodejs";

const IPA_FILENAME = "Mercurio.Driver.ipa";

function getIpaPath() {
  return path.join(process.cwd(), "assets", IPA_FILENAME);
}

function plainError(message: string, status: number) {
  return new NextResponse(message, {
    status,
    headers: {
      "Cache-Control": "no-store",
      "Content-Type": "text/plain; charset=utf-8"
    }
  });
}

export async function GET(request: Request) {
  const { searchParams } = new URL(request.url);
  const token = searchParams.get("token");
  const validation = validateDownloadToken(token);

  if (!validation.ok) {
    return plainError(validation.reason, validation.status);
  }

  const ipaPath = getIpaPath();

  try {
    await access(ipaPath);
  } catch {
    return plainError(`IPA file not found at ${ipaPath}`, 404);
  }

  const [fileBuffer, fileStats] = await Promise.all([readFile(ipaPath), stat(ipaPath)]);

  return new NextResponse(new Uint8Array(fileBuffer), {
    status: 200,
    headers: {
      "Cache-Control": "no-store",
      "Content-Type": "application/octet-stream",
      "Content-Disposition": `attachment; filename="${IPA_FILENAME}"`,
      "Content-Length": String(fileStats.size)
    }
  });
}
