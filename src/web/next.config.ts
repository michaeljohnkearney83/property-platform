import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Produces a minimal self-contained server for the Docker image (Azure Container Apps).
  output: "standalone",
  poweredByHeader: false,
};

export default nextConfig;
