const tiles = [
  { label: "Properties", hint: "Portfolio module" },
  { label: "Active tenancies", hint: "Tenancies module" },
  { label: "Rent arrears", hint: "Ledger module" },
  { label: "Certificates due (30 days)", hint: "Compliance module" },
];

export default function DashboardPage() {
  return (
    <div>
      <h1 className="mb-6 text-2xl font-semibold">Dashboard</h1>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {tiles.map((t) => (
          <div key={t.label} className="rounded-lg border border-slate-200 bg-white p-4">
            <div className="text-sm text-slate-500">{t.label}</div>
            <div className="mt-2 text-3xl font-semibold">–</div>
            <div className="mt-1 text-xs text-slate-400">{t.hint}</div>
          </div>
        ))}
      </div>
    </div>
  );
}
