// Liveness/readiness probe for Azure Container Apps.
export function GET() {
  return Response.json({ status: "ok" });
}
