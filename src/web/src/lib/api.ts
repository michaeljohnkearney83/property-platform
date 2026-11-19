import "server-only";

/**
 * Server-side API client. Calls the ASP.NET Core API from Server Components / Route Handlers,
 * so the API URL and access tokens never reach the browser.
 *
 * TODO(auth): when Entra External ID is wired up (Auth.js + Microsoft Entra provider),
 * attach the user's access token here: headers.Authorization = `Bearer ${token}`.
 * TODO(codegen): replace these hand-written types with a client generated from the API's
 * OpenAPI document (/openapi/v1.json) using Kiota or openapi-typescript.
 */
const baseUrl = process.env.API_BASE_URL ?? "http://localhost:5080";

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
  }
}

export async function apiGet<T>(path: string): Promise<T> {
  const res = await fetch(`${baseUrl}${path}`, { cache: "no-store" });
  if (!res.ok) throw new ApiError(res.status, `GET ${path} failed: ${res.status}`);
  return (await res.json()) as T;
}

export type PropertyType = "House" | "Flat" | "Hmo" | "Bungalow" | "Commercial";

export interface PropertyDto {
  id: string;
  reference: string;
  addressLine1: string;
  town: string;
  postcode: string;
  type: PropertyType;
  bedrooms: number;
}
