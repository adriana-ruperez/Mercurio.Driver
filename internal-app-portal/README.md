# Internal App Portal for Vercel Hobby

Mini app Next.js para distribuir `Mercurio.Driver.ipa` por OTA desde una web privada simple en Vercel Hobby, sin base de datos, sin OAuth y sin depender de Vercel Deployment Protection.

## 1. Que hace esta solucion

Esta carpeta contiene una app web autocontenida pensada para desplegarse en Vercel con `internal-app-portal/` como Root Directory. La app:

- muestra un login en `/`
- valida una unica credencial compartida definida por variables de entorno
- guarda una sesion web simple en cookie `HttpOnly`
- genera tokens firmados de corta duracion para `manifest.plist` e `ipa`
- sirve el `manifest.plist` desde `/api/manifest`
- sirve el `Mercurio.Driver.ipa` desde `/api/ipa`
- inicia la instalacion iOS con `itms-services://`

## 2. Como funciona el login

1. El usuario abre `/`.
2. Si no hay cookie de sesion valida, se muestra el formulario de login.
3. `POST /api/login` compara el usuario y la contraseña con:
   - `PORTAL_USERNAME`
   - `PORTAL_PASSWORD`
4. Si son correctos, el servidor crea una cookie de sesion firmada.
5. La pagina `/download` solo se muestra si la cookie sigue siendo valida.
6. Desde `/download`, el servidor genera un token firmado de corta duracion para el manifest y el IPA.

No hay base de datos ni usuarios individuales.

## 3. Por que se usan tokens firmados en vez de cookies para el manifest y el ipa

El login del navegador usa una cookie simple porque es la forma minima de mantener la sesion web entre `/` y `/download`.

La descarga OTA usa tokens firmados en la URL porque:

- el flujo `itms-services` no debe depender de una sesion de navegador compleja
- iOS puede solicitar el `manifest.plist` y el `.ipa` fuera del flujo normal del formulario web
- el token firmado permite proteger tanto `manifest` como `ipa` con la misma comprobacion en servidor
- la expiracion corta reduce la ventana de reutilizacion del enlace

La firma usa HMAC SHA-256 con `TOKEN_SECRET`.

## 4. Estructura creada

- [package.json](/Users/adria/Mercurio.Driver/internal-app-portal/package.json)
- [tsconfig.json](/Users/adria/Mercurio.Driver/internal-app-portal/tsconfig.json)
- [next.config.mjs](/Users/adria/Mercurio.Driver/internal-app-portal/next.config.mjs)
- [.env.example](/Users/adria/Mercurio.Driver/internal-app-portal/.env.example)
- [app/page.tsx](/Users/adria/Mercurio.Driver/internal-app-portal/app/page.tsx)
- [app/download/page.tsx](/Users/adria/Mercurio.Driver/internal-app-portal/app/download/page.tsx)
- [app/api/login/route.ts](/Users/adria/Mercurio.Driver/internal-app-portal/app/api/login/route.ts)
- [app/api/logout/route.ts](/Users/adria/Mercurio.Driver/internal-app-portal/app/api/logout/route.ts)
- [app/api/manifest/route.ts](/Users/adria/Mercurio.Driver/internal-app-portal/app/api/manifest/route.ts)
- [app/api/ipa/route.ts](/Users/adria/Mercurio.Driver/internal-app-portal/app/api/ipa/route.ts)
- [app/layout.tsx](/Users/adria/Mercurio.Driver/internal-app-portal/app/layout.tsx)
- [app/globals.css](/Users/adria/Mercurio.Driver/internal-app-portal/app/globals.css)
- [lib/env.ts](/Users/adria/Mercurio.Driver/internal-app-portal/lib/env.ts)
- [lib/auth.ts](/Users/adria/Mercurio.Driver/internal-app-portal/lib/auth.ts)
- [assets/Mercurio.Driver.ipa](/Users/adria/Mercurio.Driver/internal-app-portal/assets/Mercurio.Driver.ipa)

## 5. Donde colocar `Mercurio.Driver.ipa`

Debes sustituir el placeholder:

- [assets/Mercurio.Driver.ipa](/Users/adria/Mercurio.Driver/internal-app-portal/assets/Mercurio.Driver.ipa)

por el IPA Enterprise real, manteniendo exactamente el mismo nombre:

`Mercurio.Driver.ipa`

## 6. Variables de entorno

Debes configurar estas variables tanto en local como en Vercel:

- `PORTAL_USERNAME`
- `PORTAL_PASSWORD`
- `TOKEN_SECRET`
- `PUBLIC_BASE_URL`
- `APP_BUNDLE_ID`
- `APP_VERSION`
- `APP_TITLE`
- `TOKEN_TTL_SECONDS`

Ejemplo:

```env
PORTAL_USERNAME=driver
PORTAL_PASSWORD=change-this-now
TOKEN_SECRET=replace-with-a-long-random-secret
PUBLIC_BASE_URL=https://your-project-name.vercel.app
APP_BUNDLE_ID=com.example.mercurio.driver
APP_VERSION=1.0.0
APP_TITLE=Mercurio Driver
TOKEN_TTL_SECONDS=600
```

## 7. Como cambiar nombre visible, bundle id, version y expiracion

Se cambian solo desde variables:

- `APP_TITLE`: nombre visible en la web y en el manifest
- `APP_BUNDLE_ID`: bundle identifier del manifest
- `APP_VERSION`: version del manifest
- `TOKEN_TTL_SECONDS`: vida del token de descarga en segundos

## 8. Como desplegar en Vercel

### Opcion A: desde la interfaz web

1. Sube este repositorio a GitHub/GitLab/Bitbucket.
2. En Vercel, crea un proyecto nuevo importando el repo.
3. En `Root Directory`, selecciona `internal-app-portal`.
4. Vercel detectara Next.js automaticamente.
5. En `Settings > Environment Variables`, añade todas las variables indicadas arriba.
6. Asegurate de que el archivo `internal-app-portal/assets/Mercurio.Driver.ipa` ya esta en el repo antes del deploy.
7. Despliega.

### Opcion B: con Vercel CLI

Desde `internal-app-portal/`:

```bash
npm install
npx vercel
```

En producción:

```bash
npx vercel --prod
```

## 9. Como configurar variables de entorno en Vercel

En el proyecto de Vercel:

1. Ve a `Settings`.
2. Abre `Environment Variables`.
3. Crea cada variable con su valor.
4. Guarda los cambios.
5. Redeploy si ya existia un despliegue anterior.

`PUBLIC_BASE_URL` debe coincidir exactamente con tu URL final de Vercel, por ejemplo:

`https://mercurio-driver-portal.vercel.app`

## 10. Como probar la instalacion en iPhone

1. Abre la URL HTTPS de Vercel en Safari del iPhone.
2. Inicia sesion con la credencial compartida.
3. Verifica que aparece la pantalla de descarga.
4. Pulsa `Instalar app`.
5. Acepta la instalacion cuando iOS lo solicite.
6. Espera a que aparezca el icono en el dispositivo.
7. Si en iOS 18+ la instalacion manual no termina de activarse, reinicia el iPhone y vuelve a abrir la app.

Apple indica que las instalaciones manuales de apps Enterprise en iOS 18 o posterior pueden requerir reinicio del dispositivo para completar la confianza inicial del perfil.

## 11. Errores tipicos

- `PUBLIC_BASE_URL` incorrecta:
  el enlace `itms-services` apuntara a un dominio equivocado y fallara la descarga.
- token expirado:
  `manifest` o `ipa` devolveran `401` o `403` cuando la URL caduque.
- `manifest.plist` mal formado:
  iOS no podra interpretar el manifest.
- MIME/content-type incorrecto:
  el servidor debe responder `text/xml` para el manifest y `application/octet-stream` para el IPA.
- IPA no encontrado:
  la ruta `assets/Mercurio.Driver.ipa` no existe o sigue siendo el placeholder.
- instalacion OTA falla en iOS 18+:
  puede requerir reiniciar el iPhone tras la instalacion manual.
- `APP_BUNDLE_ID` o `APP_VERSION` incorrectos:
  el manifest no reflejara el binario real.

## 12. Como cambiar el usuario y contraseña

Solo cambia:

- `PORTAL_USERNAME`
- `PORTAL_PASSWORD`

en las variables de entorno de Vercel y vuelve a desplegar o vuelve a promover un deploy con las nuevas variables.

## 13. Advertencia de seguridad

Esta solucion usa una sola credencial compartida y no ofrece control individual por usuario. Eso significa:

- cualquiera con esa credencial puede acceder al portal
- si la credencial se comparte fuera de la empresa, no hay trazabilidad individual

Como solucion minima temporal para muy pocas personas puede ser aceptable, pero no sustituye un sistema real de identidad.

## 14. Requisitos de Apple cubiertos

Esta implementacion respeta los puntos clave del flujo OTA:

- usa `itms-services://?action=download-manifest&url=...`
- genera `manifest.plist`
- sirve el `.ipa`
- no muestra enlace directo visible al `.ipa`
- exige HTTPS a traves de la URL publica de Vercel
- restringe `manifest` e `ipa` a usuarios autenticados mediante token firmado

Referencia oficial:

- Apple Support: Distribute proprietary in-house apps to Apple devices
  https://support.apple.com/en-lamr/guide/deployment/depce7cefc4d/web

## 15. Notas sobre Vercel Hobby

Esta solucion no usa Vercel Deployment Protection. La proteccion la implementa la propia app con:

- login server-side
- cookie de sesion minima
- tokens HMAC firmados con expiracion corta para descargas

La URL `*.vercel.app` ya sale en HTTPS automaticamente desde Vercel.
