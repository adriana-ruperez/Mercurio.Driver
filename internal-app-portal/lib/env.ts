const REQUIRED_ENV_KEYS = [
  "PORTAL_USERNAME",
  "PORTAL_PASSWORD",
  "TOKEN_SECRET",
  "PUBLIC_BASE_URL",
  "APP_BUNDLE_ID",
  "APP_VERSION",
  "APP_TITLE",
  "TOKEN_TTL_SECONDS"
] as const;

type RequiredEnvKey = (typeof REQUIRED_ENV_KEYS)[number];

function getRequiredEnv(name: RequiredEnvKey): string {
  const value = process.env[name];

  if (!value) {
    throw new Error(`Missing required environment variable: ${name}`);
  }

  return value;
}

function getPositiveInt(name: "TOKEN_TTL_SECONDS"): number {
  const raw = getRequiredEnv(name);
  const parsed = Number.parseInt(raw, 10);

  if (!Number.isFinite(parsed) || parsed <= 0) {
    throw new Error(`${name} must be a positive integer`);
  }

  return parsed;
}

function getPublicBaseUrl(): string {
  const raw = getRequiredEnv("PUBLIC_BASE_URL").trim();

  if (!raw.startsWith("https://")) {
    throw new Error("PUBLIC_BASE_URL must start with https://");
  }

  return raw.replace(/\/+$/, "");
}

export function getPortalConfig() {
  return {
    username: getRequiredEnv("PORTAL_USERNAME"),
    password: getRequiredEnv("PORTAL_PASSWORD"),
    tokenSecret: getRequiredEnv("TOKEN_SECRET"),
    publicBaseUrl: getPublicBaseUrl(),
    appBundleId: getRequiredEnv("APP_BUNDLE_ID"),
    appVersion: getRequiredEnv("APP_VERSION"),
    appTitle: getRequiredEnv("APP_TITLE"),
    tokenTtlSeconds: getPositiveInt("TOKEN_TTL_SECONDS")
  };
}
