import { cookies } from "next/headers";
import { redirect } from "next/navigation";

import { createDownloadToken, getSessionCookieName, validateSessionToken } from "@/lib/auth";
import { getPortalConfig } from "@/lib/env";

export const dynamic = "force-dynamic";

export default async function DownloadPage() {
  const cookieStore = await cookies();
  const sessionToken = cookieStore.get(getSessionCookieName())?.value;
  const sessionResult = validateSessionToken(sessionToken);

  if (!sessionResult.ok) {
    redirect("/");
  }

  const { appTitle, appVersion, publicBaseUrl, tokenTtlSeconds } = getPortalConfig();
  const downloadToken = createDownloadToken();
  const manifestUrl = `${publicBaseUrl}/api/manifest?token=${encodeURIComponent(downloadToken)}`;
  const installUrl = `itms-services://?action=download-manifest&url=${encodeURIComponent(manifestUrl)}`;

  return (
    <main className="page-shell">
      <section className="card hero">
        <span className="eyebrow">Descarga autorizada</span>
        <h1>{appTitle}</h1>
        <p>
          App interna para conductores. Esta sesion permite obtener temporalmente el manifest OTA y el archivo
          IPA firmado. El enlace de instalacion expira en {tokenTtlSeconds} segundos.
        </p>

        <ul className="meta-list">
          <li>Version: {appVersion}</li>
          <li>Acceso firmado de corta duracion</li>
          <li>Portal preparado para Vercel Hobby</li>
        </ul>

        <div className="button-row" style={{ marginTop: 24 }}>
          <a className="button button-primary" href={installUrl}>
            Instalar app
          </a>

          <form action="/api/logout" method="post">
            <button className="button button-secondary" type="submit">
              Cerrar sesion
            </button>
          </form>
        </div>

        <div className="panel-grid">
          <article className="panel">
            <h2>Instrucciones</h2>
            <ol>
              <li>Abre esta pagina desde Safari en el iPhone.</li>
              <li>Pulsa <strong>Instalar app</strong>.</li>
              <li>Acepta la descarga cuando iOS lo solicite.</li>
              <li>Espera a que el icono aparezca en la pantalla de inicio.</li>
              <li>No compartas esta URL ni la credencial fuera de la empresa.</li>
            </ol>
          </article>

          <article className="panel">
            <h2>Aviso para iOS 18+</h2>
            <p>
              Si la instalacion manual OTA no termina de activarse correctamente, reinicia el iPhone y vuelve
              a abrir la app. Apple indica que esto puede ser necesario en iOS 18 o posterior para apps
              Enterprise instaladas manualmente.
            </p>
          </article>
        </div>

        <div className="warning-box" style={{ marginTop: 22 }}>
          El boton de instalacion usa <code>itms-services</code> con un <code>manifest.plist</code> generado en
          servidor. No se muestra ningun enlace directo visible al archivo <code>.ipa</code>.
        </div>
      </section>
    </main>
  );
}
