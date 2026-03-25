import { cookies } from "next/headers";
import { redirect } from "next/navigation";

import { getSessionCookieName, validateSessionToken } from "@/lib/auth";
import { getPortalConfig } from "@/lib/env";

export const dynamic = "force-dynamic";

type HomePageProps = {
  searchParams?: Promise<{
    error?: string;
  }>;
};

const ERROR_MESSAGES: Record<string, string> = {
  invalid_credentials: "Usuario o contraseña incorrectos.",
  missing_credentials: "Debes introducir usuario y contraseña."
};

export default async function HomePage({ searchParams }: HomePageProps) {
  const cookieStore = await cookies();
  const sessionToken = cookieStore.get(getSessionCookieName())?.value;
  const sessionResult = validateSessionToken(sessionToken);

  if (sessionResult.ok) {
    redirect("/download");
  }

  const params = (await searchParams) ?? {};
  const { appTitle } = getPortalConfig();
  const errorMessage = params.error ? ERROR_MESSAGES[params.error] ?? "No se pudo iniciar sesion." : null;

  return (
    <main className="page-shell">
      <section className="login-layout">
        <div className="card hero">
          <span className="eyebrow">Distribucion interna</span>
          <h1>{appTitle}</h1>
          <p>
            Portal privado para instalar la app interna de conductores en iPhone mediante distribucion OTA
            Enterprise. El acceso usa una unica credencial compartida y el portal emite enlaces firmados de
            corta duracion para el manifest y el IPA.
          </p>

          <ul className="meta-list">
            <li>Instalacion OTA sobre HTTPS</li>
            <li>Sin App Store publica</li>
            <li>Acceso privado con login simple</li>
          </ul>

          <div className="panel-grid">
            <article className="panel">
              <h2>Como funciona</h2>
              <p>
                Inicia sesion con la credencial interna. Si es correcta, accederas a una pantalla de descarga
                que genera un enlace firmado para instalar la app desde Safari.
              </p>
            </article>

            <article className="panel">
              <h2>Aviso para iOS 18+</h2>
              <p>
                En instalaciones manuales OTA, algunos iPhone pueden requerir reiniciar el dispositivo despues
                de descargar la app para completar correctamente la activacion inicial.
              </p>
            </article>
          </div>
        </div>

        <aside className="card login-card">
          <span className="eyebrow">Acceso</span>
          <h2 style={{ marginTop: 16, marginBottom: 10 }}>Iniciar sesion</h2>
          <p className="footnote" style={{ marginTop: 0, marginBottom: 18 }}>
            Usa la credencial compartida configurada en Vercel para acceder al portal.
          </p>

          {errorMessage ? (
            <div className="error-box" style={{ marginBottom: 16 }}>
              {errorMessage}
            </div>
          ) : null}

          <form action="/api/login" method="post" className="form-stack">
            <div className="field">
              <label htmlFor="username">Usuario</label>
              <input id="username" name="username" type="text" autoComplete="username" required />
            </div>

            <div className="field">
              <label htmlFor="password">Contraseña</label>
              <input id="password" name="password" type="password" autoComplete="current-password" required />
            </div>

            <div className="button-row">
              <button className="button button-primary" type="submit">
                Entrar
              </button>
            </div>
          </form>
        </aside>
      </section>
    </main>
  );
}
