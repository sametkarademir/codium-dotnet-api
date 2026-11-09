# Codium .NET Template

A `dotnet new` template for an N-layer ASP.NET Core Web API (Domain, Application, EF Core / PostgreSQL, ASP.NET Core Identity, JWT, Hangfire, Serilog) with unit, integration (Testcontainers) and Hurl parity tests.

## Repository layout

| Path | Purpose |
|---|---|
| `templates/codium-api/` | **Everything that is shipped to users.** Only this folder ends up in the template. |
| `.github/workflows/` | CI for this repository (builds and tests the template content). |
| `.claude/`, `.specify/`, `specs/` | Development tooling for working on the template itself. Never shipped. |

## Using the template

Local development install (from a clone of this repository):

```bash
dotnet new install ./templates/codium-api
dotnet new codium -n Acme.Shop
```

Uninstall with `dotnet new uninstall <path>`.

### Parameters

| Parameter | Default | Description |
|---|---|---|
| `-n, --name` | folder name | Project / namespace root. Also drives the database name and display names. |
| `--AdminEmail` | `admin@example.com` | E-mail of the seeded administrator account. |

The JWT signing key and the Hangfire dashboard password are generated randomly for every new project.

## Working on the template

The content under `templates/codium-api/` is a normal, buildable solution:

```bash
cd templates/codium-api
dotnet build Codium.Template.sln
dotnet test tests/Codium.Template.UnitTests
```

Integration and parity tests need Docker / PostgreSQL; see `templates/codium-api/tests/parity/README.md`.

Token strings such as `Codium.Template`, `codium_service`, `admin@example.com` and the sample JWT key are *replacement tokens* declared in `templates/codium-api/.template.config/template.json`. Keep them intact in the source so generation keeps working.
