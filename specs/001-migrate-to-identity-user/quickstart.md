# Quickstart: Validate the Identity Migration

## Prerequisites

- .NET 9 runtime/SDK, Docker (integration tests start a PostgreSQL container), `hurl` (parity checks).

## New database (only supported path; no existing DB to convert)

1. `dotnet build Codium.sln`
2. The template ships no migrations. For a real project generate your own initial migration
   (`dotnet ef migrations add InitialCreate -p src/Codium.Template.EntityFrameworkCore -s src/Codium.Template.HttpApi.Host`)
   so `DbMigrationInitializer` can apply it and seed development data. Without a migration it applies nothing.
3. Log in with the seeded admin (`admin@codium.com` / `Pp123456*`); the response shape is unchanged
   ([contracts/api-parity.md](contracts/api-parity.md)).

## Automated checks (test pyramid)

| Layer | Project / files | Needs | Covers |
|-------|-----------------|-------|--------|
| Unit | `tests/Codium.Template.UnitTests` | nothing (no Docker, no database) | Pure logic: password policy vs Identity, `IdentityResult` mapping, DTO validators, translation completeness, JWT generation, `CurrentUser`, claim/query/normalization helpers, AutoMapper profile, `AuthAppService.LoginAsync` error mapping |
| Integration | `tests/Codium.Template.IntegrationTests` | Docker (Testcontainers PostgreSQL, schema via `Database.EnsureCreated()`) | What needs a real database: seeding, audit fields, soft delete, filtered unique indexes, role/permission flows over HTTP |
| API contract / parity | `tests/parity/*.hurl` | running API + fresh database + `hurl` | Black-box HTTP contract against the pre-Identity baseline (`tests/parity/baseline/`), see `tests/parity/README.md` |

Commands:

- Unit: `dotnet test tests/Codium.Template.UnitTests`
- Integration: `dotnet test tests/Codium.Template.IntegrationTests`
- Parity: `tests/parity/run.sh /tmp/current && diff -r tests/parity/baseline /tmp/current` (no output means parity)

The application services (`UserAppService`, `RoleAppService`, `ProfileAppService`) are deliberately not unit tested with mocked
managers: they delegate to Identity and EF, so their behavior is verified by the integration and parity layers.

## Behaviors worth checking by hand

- Email and role-name lookups are case-insensitive through Identity's normalizer (`ADMIN@Codium.com` logs in).
  Identity's default normalization does not strip accents, unlike the former `NormalizeValue()` helper, so accented
  emails/role names are now accent-sensitive (research R2).
- Lockout: the fifth wrong password already answers 403 "locked"; admin `unlock` restores login.
- Inactive user and unconfirmed email are denied with 403.
- `ShouldChangePasswordOnNextLogin` is exposed on the profile but was never enforced at login before and still is not.
- Removing a role from a user deletes the Identity `UserRoles` row (no soft-delete for the link).
