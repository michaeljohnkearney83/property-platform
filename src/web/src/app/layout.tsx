import type { Metadata } from "next";
import Link from "next/link";
import "./globals.css";

export const metadata: Metadata = {
  title: "Property Platform",
  description: "Property management – portfolio, tenancies, accounts, compliance and maintenance.",
};

const nav = [
  { href: "/", label: "Dashboard" },
  { href: "/properties", label: "Properties" },
  { href: "/tenancies", label: "Tenancies" },
  { href: "/accounts", label: "Accounts" },
  { href: "/compliance", label: "Compliance" },
  { href: "/maintenance", label: "Maintenance" },
];

const version = process.env.NEXT_PUBLIC_APP_VERSION ?? "0.0.0";
const commit = process.env.NEXT_PUBLIC_APP_COMMIT;

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en-GB">
      <body className="min-h-screen bg-slate-50 text-slate-900 antialiased">
        <div className="flex min-h-screen">
          <aside className="hidden w-56 shrink-0 border-r border-slate-200 bg-white p-4 md:block">
            <div className="mb-6 text-lg font-semibold">Property Platform</div>
            <nav className="flex flex-col gap-1">
              {nav.map((item) => (
                <Link
                  key={item.href}
                  href={item.href}
                  className="rounded px-3 py-2 text-sm text-slate-700 hover:bg-slate-100"
                >
                  {item.label}
                </Link>
              ))}
            </nav>
          </aside>
          <div className="flex flex-1 flex-col">
            <main className="flex-1 p-4 md:p-8">{children}</main>
            <footer className="border-t border-slate-200 px-4 py-2 text-xs text-slate-400 md:px-8">
              Property Platform v{version}
              {commit && <span title="Build commit"> ({commit})</span>}
            </footer>
          </div>
        </div>
      </body>
    </html>
  );
}
