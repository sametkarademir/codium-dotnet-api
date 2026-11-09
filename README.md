# Codium .NET Template

A `dotnet new` template for an N-layer ASP.NET Core Web API (Domain, Application, EF Core / PostgreSQL, ASP.NET Core Identity, JWT, Hangfire, Serilog) with unit, integration (Testcontainers) and Hurl parity tests.

## Repository layout

| Path | Purpose |
|---|---|
| `templates/codium-api/` | **Everything that is shipped to users.** Only this folder ends up in the template. |
| `.github/workflows/` | CI for this repository (builds and tests the template content). |
| `.claude/`, `.specify/`, `specs/` | Development tooling for working on the template itself. Never shipped. |

## Installing from GitHub Packages

The template is published as the NuGet package `Codium.Templates` to GitHub Packages
(`https://nuget.pkg.github.com/sametkarademir/index.json`) whenever a `v*` tag is pushed.
GitHub Packages needs authentication even for reading.

1. Create a personal access token (classic) with the `read:packages` scope.
2. Register the feed once (the clear-text flag is required on macOS/Linux):

   ```bash
   dotnet nuget add source "https://nuget.pkg.github.com/sametkarademir/index.json" \
     --name github-sametkarademir \
     --username sametkarademir \
     --password <TOKEN> \
     --store-password-in-clear-text
   ```

3. Install, update and use:

   ```bash
   dotnet new install Codium.Templates      # install the latest version
   dotnet new install Codium.Templates::1.2.0   # a specific version
   dotnet new update                         # update installed templates
   dotnet new codium -n Acme.Shop
   ```

## Releasing

1. Merge your changes to `main` (CI must be green, including the *Template smoke* job).
2. Tag and push: `git tag v1.2.3 && git push origin v1.2.3`.
3. The *Release* workflow derives the version from the tag, runs the smoke job on that exact
   package and then publishes it. Check the result under the repository's *Packages* section.

## Using the template from a clone

Local development install:

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
