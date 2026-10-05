# Implementation Plan: Migrate Custom User to Microsoft Identity User

**Branch**: `001-migrate-to-identity-user` | **Date**: 2026-09-29 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-migrate-to-identity-user/spec.md`

## Summary

Replace the template's hand-written `User`, `Role` and `UserRole` entities with ASP.NET Core Identity types (`IdentityUser<Guid>`, `IdentityRole<Guid>`, `IdentityUserRole<Guid>`) while keeping every HTTP endpoint, DTO, `Result<T>` envelope, JWT/refresh-token flow, session logic and permission model unchanged. The approach is deliberately incremental: swap the entity base types and persistence mapping, register Identity core services with the existing `UserConsts` policies, keep the current app services and repositories, and route all user/role normalization through Identity's own normalizer. No data migration is needed because the template has no existing database. See [research.md](research.md) for decisions.

## Technical Context

**Language/Version**: C# / .NET 9 (`net9.0`, all 8 projects)

**Primary Dependencies**: EF Core 9.0.14, Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4, `Microsoft.Extensions.Identity.Core` 9.0.14 (already in Domain), JwtBearer 9.0.14, AutoMapper, FluentValidation, Hangfire.PostgreSql. New: `Microsoft.Extensions.Identity.Stores` (Domain, for `IdentityUser`/`IdentityRole`) and `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 9.0.x (EntityFrameworkCore project).

**Storage**: PostgreSQL. Table prefix/schema from `ApplicationConsts.DbTablePrefix` / `DbSchema`. The template ships no database or committed EF migrations, so only new databases are supported (see research R6).

**Testing**: No test project exists in the repo today. Plan adds an integration test project plus reuses Hurl-style functional plans (`hurl-test-plan` skill) for endpoint parity checks.

**Target Platform**: Linux/macOS server (ASP.NET Core Web API, versioned controllers `v1`).

**Project Type**: web-service (layered: Domain.Shared → Domain → Application.Contracts → Application → EntityFrameworkCore → HttpApi → HttpApi.Host).

**Performance Goals**: No regression versus current login/list latency (parity; no new targets).

**Constraints**: Zero change to public API contracts; `Guid` user/role IDs unchanged; soft-delete + audit + multi-tenant query filters keep working; user/role normalization via Identity only.

**Scale/Scope**: Template-level; touches ~25 files across Domain, EntityFrameworkCore, Application, Host.

## Constitution Check

`.specify/memory/constitution.md` is still the unfilled template, so no ratified principles or gates exist. **Result: no gates to evaluate (PASS by absence).** Recommendation: run `/speckit-constitution` before implementation if the team wants enforceable rules (e.g., test-first). Re-check after Phase 1: no change.

## Project Structure

### Documentation (this feature)

```text
specs/001-migrate-to-identity-user/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── api-parity.md
└── tasks.md             # created later by /speckit-tasks
```

### Source Code (repository root)

```text
src/
├── Codium.Template.Domain.Shared/     # UserConsts kept; audit interfaces reused
├── Codium.Template.Domain/
│   ├── Users/User.cs                  # -> IdentityUser<Guid> + audit interfaces + custom props
│   ├── Roles/Role.cs                  # -> IdentityRole<Guid> + audit interfaces + Description
│   ├── UserRoles/UserRole.cs          # -> IdentityUserRole<Guid> (+creation audit)
│   ├── Repositories/                  # signatures kept
│   └── DevelopmentDataSeederContributor.cs
├── Codium.Template.EntityFrameworkCore/
│   ├── Contexts/ApplicationDbContext.cs   # -> IdentityDbContext<User, Role, Guid, ..., UserRole, ...>
│   ├── EntityConfigurations/{User,Role,UserRole}Configuration.cs
│   ├── Repositories/{User,Role,UserRole}Repository.cs   # use ILookupNormalizer instead of NormalizeValue()
│   └── ServiceCollectionExtensions.cs
├── Codium.Template.Application/
│   ├── ServiceCollectionExtensions.cs # AddIdentityCore<User>().AddRoles<Role>().AddEntityFrameworkStores
│   └── {Auth,Users,Roles,Profiles}/   # minimal edits (ILookupNormalizer, UserRole usage)
└── Codium.Template.HttpApi.Host/      # no contract changes

tests/                                 # new: integration tests for auth/user/role parity
```

**Structure Decision**: Keep the existing layered solution; no new production projects. Add one test project.

## Complexity Tracking

| Deviation | Why needed | Simpler alternative rejected because |
|-----------|------------|--------------------------------------|
| `UserRole` loses its own `Id` and soft-delete (Identity composite key) | Identity's role store requires `IdentityUserRole<Guid>` shape | Keeping a surrogate key/soft-delete would fight Identity's `UserManager` role operations |
| Audit interfaces re-implemented on `User`/`Role` | C# single inheritance: cannot inherit both `IdentityUser` and `FullAuditedEntity` | Wrapper/one-to-one profile table would change queries and break "existing structure" goal |
