# ADR 0001: Technology stack, source control, delivery and hosting

- **Status:** Accepted
- **Date:** 2026-11-19

## Context

We are building a new multi-tenant property management SaaS from scratch, starting with one lead developer and growing to a small team. We need a stack the team can hire for, a single cloud, and a delivery process with traceability from work items to production.

## Decision

- **Backend:** C# / .NET 10 (LTS) with ASP.NET Core minimal APIs and EF Core + Npgsql, built as a **modular monolith**.
- **Frontend:** Next.js (TypeScript, App Router). The browser calls Next.js; Next.js calls the API server-side.
- **Database:** PostgreSQL (Azure Database for PostgreSQL Flexible Server).
- **Source control:** GitHub.
- **Agile and CI/CD:** Azure DevOps Boards (Agile process) and Azure Pipelines, connected to GitHub with the Boards and Pipelines GitHub apps.
- **Hosting:** Azure only, UK South as primary and UK West for geo-backup. Container Apps, Key Vault, Blob Storage, Front Door + WAF, and Application Insights.
- **Identity:** Microsoft Entra External ID for landlords, tenants and contractors; Entra ID for staff SSO.

## Consequences

- One cloud and one identity platform reduce operational surface. In return, we accept Azure lock-in.
- The split tooling (GitHub for code, Azure DevOps for boards and pipelines) needs the GitHub apps installed and `AB#` linking discipline.
- A modular monolith keeps deployment simple. Module boundaries are enforced by tests so that modules can be extracted later.
