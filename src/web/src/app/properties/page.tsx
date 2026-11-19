import { apiGet, type PropertyDto } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function PropertiesPage() {
  let properties: PropertyDto[] = [];
  let error: string | null = null;

  try {
    properties = await apiGet<PropertyDto[]>("/api/portfolio/properties");
  } catch (e) {
    error = e instanceof Error ? e.message : "Could not load properties.";
  }

  return (
    <div>
      <h1 className="mb-6 text-2xl font-semibold">Properties</h1>

      {error && (
        <p className="mb-4 rounded border border-amber-300 bg-amber-50 p-3 text-sm text-amber-800">
          {error} – is the API running?
        </p>
      )}

      <div className="overflow-x-auto rounded-lg border border-slate-200 bg-white">
        <table className="w-full text-left text-sm">
          <thead className="border-b border-slate-200 bg-slate-50 text-slate-600">
            <tr>
              <th className="px-4 py-2">Reference</th>
              <th className="px-4 py-2">Address</th>
              <th className="px-4 py-2">Town</th>
              <th className="px-4 py-2">Postcode</th>
              <th className="px-4 py-2">Type</th>
              <th className="px-4 py-2 text-right">Beds</th>
            </tr>
          </thead>
          <tbody>
            {properties.length === 0 && !error && (
              <tr>
                <td colSpan={6} className="px-4 py-6 text-center text-slate-500">
                  No properties yet.
                </td>
              </tr>
            )}
            {properties.map((p) => (
              <tr key={p.id} className="border-b border-slate-100 last:border-0">
                <td className="px-4 py-2 font-medium">{p.reference}</td>
                <td className="px-4 py-2">{p.addressLine1}</td>
                <td className="px-4 py-2">{p.town}</td>
                <td className="px-4 py-2">{p.postcode}</td>
                <td className="px-4 py-2">{p.type}</td>
                <td className="px-4 py-2 text-right">{p.bedrooms}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
