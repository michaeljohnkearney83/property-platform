import type { NextConfig } from "next";
import pkg from "./package.json";

const commit = (process.env.SOURCE_REVISION ?? "").slice(0, 7);

const nextConfig: NextConfig = {
  // Produces a minimal self-contained server for the Docker image (Azure Container Apps).
  output: "standalone",
  poweredByHeader: false,
  // Inlined at build time. The version is managed by release-please (package.json "version").
  env: {
    NEXT_PUBLIC_APP_VERSION: pkg.version,
    NEXT_PUBLIC_APP_COMMIT: commit,
  },
};

export default nextConfig;
