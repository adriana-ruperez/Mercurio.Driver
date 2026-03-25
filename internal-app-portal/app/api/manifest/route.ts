import { NextResponse } from "next/server";

import { validateDownloadToken } from "@/lib/auth";
import { getPortalConfig } from "@/lib/env";

export const dynamic = "force-dynamic";

function unauthorized(status: 401 | 403, message: string) {
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
    return unauthorized(validation.status, validation.reason);
  }

  const { appBundleId, appTitle, appVersion, publicBaseUrl } = getPortalConfig();
  const ipaUrl = `${publicBaseUrl}/api/ipa?token=${encodeURIComponent(token!)}`;
  const manifest = `<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>items</key>
  <array>
    <dict>
      <key>assets</key>
      <array>
        <dict>
          <key>kind</key>
          <string>software-package</string>
          <key>url</key>
          <string>${ipaUrl}</string>
        </dict>
      </array>
      <key>metadata</key>
      <dict>
        <key>bundle-identifier</key>
        <string>${appBundleId}</string>
        <key>bundle-version</key>
        <string>${appVersion}</string>
        <key>kind</key>
        <string>software</string>
        <key>title</key>
        <string>${appTitle}</string>
      </dict>
    </dict>
  </array>
</dict>
</plist>`;

  return new NextResponse(manifest, {
    status: 200,
    headers: {
      "Cache-Control": "no-store",
      "Content-Type": "text/xml; charset=utf-8",
      "Content-Disposition": 'inline; filename="manifest.plist"'
    }
  });
}
