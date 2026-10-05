# Research: Migrate Custom User to Microsoft Identity User

## R1 - Entity base types

- **Decision**: `User : IdentityUser<Guid>`, `Role : IdentityRole<Guid>`, `UserRole : IdentityUserRole<Guid>`; each re-implements the template's audit/soft-delete interfaces (`IFullAuditedObject`, `ISoftDelete`, `IEntity<Guid>`) directly.
- **Rationale**: Identity stores and `IdentityDbContext` are generic over these bases; the codebase's audit interceptors and `ApplyGlobalEntityConfigurations` work through interfaces (reflection on `ISoftDelete`, `ICreationAuditedObject`, `IEntity<>`), so they keep working. `Guid` keys match today's IDs (spec assumption).
- **Alternatives**: separate profile table linked 1:1 to `IdentityUser` (rejected: changes every query/DTO mapping); keep custom entity and only call Identity APIs (rejected: does not satisfy FR-001).
- **Watch-outs**: `IConcurrencyStamp.ConcurrencyStamp` is non-null `string` while `IdentityUser.ConcurrencyStamp` is `string?` -> explicit interface implementation. Existing `Email`/`NormalizedEmail`/`PasswordHash`/lockout/2FA/phone properties already match Identity names, so they are removed from `User` and inherited (`NormalizedEmail` is `string?` in Identity; `Email` too). `UserName`/`NormalizedUserName` are new required values: populate with the email.

## R2 - Normalization

- **Decision**: For `User` and `Role`, rely entirely on Identity's `ILookupNormalizer` (default: `ToUpperInvariant`). The template's `NormalizeValue()` is no longer used for user/role values. It stays only for non-Identity entities (`Permission`).
- **Where it changes** (all current `NormalizeValue()` call sites for users/roles):
  - `UserAppService` (search filter, create), `RoleAppService` (search filters, create, update)
  - `UserRepository` and `RoleRepository` (email/name lookups, existence checks)
  - `DevelopmentDataSeederContributor` (role names, admin role, admin email)
  - Preferred mechanism: inject `ILookupNormalizer` (`NormalizeEmail`/`NormalizeName`) where a normalized value is needed; when creating users/roles let Identity stores/managers set `NormalizedEmail`/`NormalizedUserName`/`NormalizedName`.
- **Behavior note**: `NormalizeValue()` also strips diacritics (e.g. `İ`, accents), Identity's default does not. Lookups of accented emails/role names become accent-sensitive. Accepted per decision to trust Identity.
- **Rationale**: One normalization source avoids stored-vs-lookup mismatches with `UserManager`/`RoleManager`.

## R3 - DbContext and mapping

- **Decision**: `ApplicationDbContext : IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>`. Keep `Users`, `Roles`, `UserRoles` table names via existing configurations (`Users`, `Roles`, `UserRoles`, prefix/schema kept). Claims/logins/tokens tables are created but unused (out of scope). Keep filtered unique index on `NormalizedEmail` where not deleted; Identity's default unique index on `NormalizedUserName`/`NormalizedName` must be made soft-delete-aware (filtered) to preserve current behavior of reusing a deleted user's email/role name.
- **Alternatives**: `IdentityDbContext` default table names (rejected: renames tables away from the template's current naming convention).

## R4 - Identity services registration

- **Decision**: `AddIdentityCore<User>(opts from UserConsts).AddRoles<Role>().AddEntityFrameworkStores<ApplicationDbContext>()` (no `AddIdentity`, so cookie auth is not added; JWT stays). Keep `IPasswordHasher<User>` (already Identity's `PasswordHasher<User>`, so existing hashes verify - confirmed by code search).
- **Rationale**: Gives `UserManager`/`RoleManager` for future phases while phase 1 keeps existing app-service logic (lockout counters, forced password change, session limit) to guarantee behavioral parity (FR-003, FR-004).
- **Alternatives**: rewrite Auth/User services on `SignInManager`/`UserManager` now (rejected: highest regression risk, contradicts "not break the current structure").

## R5 - UserRole semantics change

- **Decision**: `UserRole` becomes Identity's composite-key row (`UserId`,`RoleId`); rows are hard-deleted on role sync; creation audit fields kept. `IUserRoleRepository` signatures unchanged.
- **Rationale**: Identity stores do not support surrogate key/soft-delete on the join. Effective permissions unchanged (spec SC-003). Internal difference only; no API impact.

## R6 - Database and migrations

- **Finding**: The template has no existing database and no committed EF migrations, so there is nothing to convert.
- **Decision**: No upgrade script, no data conversion, no rollback procedure. The schema for the Identity-based model is created on a new database by the consumer generating the initial migration (same workflow as today). Automated tests create the schema with `Database.EnsureCreated()` so no migration needs to be committed. Removed from scope: former FR-009/FR-010 data migration requirements and User Story 3 "existing data".

## R7 - Seeder and dev data

- **Decision**: `DevelopmentDataSeederContributor` sets `UserName`, `SecurityStamp` (Guid) and uses `UserRole` composite rows; password still hashed via `IPasswordHasher<User>`.

## R8 - Testing

- **Decision**: add an integration test project (WebApplicationFactory + PostgreSQL via Testcontainers or a test DB) covering: login, refresh, lockout, forced password change, user CRUD, role sync, permission checks, profile change passwordand a fresh-database startup/seed test. Also derive Hurl parity plans from existing endpoints.
