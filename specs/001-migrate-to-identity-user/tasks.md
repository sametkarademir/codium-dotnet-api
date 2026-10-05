---

description: "Task list for migrating the custom User/Role/UserRole to Microsoft Identity"
---

# Tasks: Migrate Custom User to Microsoft Identity User

**Input**: Design documents from `/specs/001-migrate-to-identity-user/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api-parity.md, quickstart.md

**Tests**: Requested by spec FR-012 (new tests must cover migrated behavior). A test project does not exist yet; it is created in Phase 1 and tests are added per story.

**Format**: `- [ ] [TaskID] [P?] [Story?] Description with file path`. All paths are relative to the repo root; `src/` projects are named `Codium.Template.*`.

## Phase 1: Setup

- [X] T001 Add package `Microsoft.Extensions.Identity.Stores` 9.0.14 to `src/Codium.Template.Domain/Codium.Template.Domain.csproj` (keeps existing `Microsoft.Extensions.Identity.Core`)
- [X] T002 Add package `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 9.0.14 to `src/Codium.Template.EntityFrameworkCore/Codium.Template.EntityFrameworkCore.csproj`
- [X] T003 [P] Create xUnit integration test project `tests/Codium.Template.IntegrationTests/Codium.Template.IntegrationTests.csproj` (net9.0, refs: `Microsoft.AspNetCore.Mvc.Testing`, `Testcontainers.PostgreSql`, HttpApi.Host project) and add it to `Codium.sln`; include a shared fixture that starts the Host against a Testcontainers PostgreSQL database and creates the schema with `Database.EnsureCreated()` (the template ships no migrations, research R6)

- [X] T004 Capture the parity baseline BEFORE any source change (run on the untouched `main` code): create Hurl files `tests/parity/*.hurl` covering login, refresh, logout, change password, user CRUD/reset-password/sync-roles, role CRUD/sync-permissions, permission list, session list (use the `hurl-test-plan` skill for the plan); start the app against a throwaway PostgreSQL database whose schema is created with `Database.EnsureCreated()` (dev seed data applied by `DevelopmentDataSeederContributor`); run the Hurl files and store status codes and response bodies (volatile fields such as ids, tokens and timestamps masked) in `tests/parity/baseline/`. Commit these files first so later parity tests (T023, T029) compare against real previous behavior (spec SC-001)

## Phase 2: Foundational (blocks all user stories)

**Goal**: Domain entities, DbContext, Identity registration and repositories compile on Identity types. Nothing else works until this phase is complete.

- [X] T005 Rewrite `src/Codium.Template.Domain/Users/User.cs`: `User : IdentityUser<Guid>, IEntity<Guid>, IFullAuditedObject`; remove now-inherited members (`Id`, `Email`, `NormalizedEmail`, `EmailConfirmed`, `PasswordHash`, `LockoutEnd`, `LockoutEnabled`, `AccessFailedCount`, `TwoFactorEnabled`, `PhoneNumber`, `PhoneNumberConfirmed`); keep `FirstName`, `LastName`, `IsActive = true`, `ShouldChangePasswordOnNextLogin`, `PasswordChangedTime`, navigations `UserRoles`/`RefreshTokens`/`Sessions`; re-add audit properties (`CreationTime`, `CreatorId`, `LastModificationTime`, `LastModifierId`, `IsDeleted`, `DeleterId`, `DeletionTime`) with `[DisableAuditLog]`; implement `IConcurrencyStamp.ConcurrencyStamp` explicitly over Identity's nullable `ConcurrencyStamp` (data-model.md)
- [X] T006 [P] Rewrite `src/Codium.Template.Domain/Roles/Role.cs`: `Role : IdentityRole<Guid>, IEntity<Guid>, IFullAuditedObject`; keep `Description` (nullable), navigations `UserRoles`/`RolePermissions`; remove inherited `Name`/`NormalizedName`; same audit properties and explicit `IConcurrencyStamp` as T005
- [X] T007 [P] Rewrite `src/Codium.Template.Domain/UserRoles/UserRole.cs`: `UserRole : IdentityUserRole<Guid>, IEntity, ICreationAuditedObject`; composite key `UserId`+`RoleId` (no `Id`, no soft-delete); keep `User`/`Role` navigations and creation audit fields (research R5)
- [X] T008 Update `src/Codium.Template.Domain/Repositories/IUserRoleRepository.cs` to derive from non-keyed `IRepository<UserRole>` (UserRole has no `Id`); keep both method signatures unchanged
- [X] T009 Update `src/Codium.Template.EntityFrameworkCore/Repositories/UserRoleRepository.cs` to derive from `EfRepositoryBase<UserRole, ApplicationDbContext>` and implement `IUserRoleRepository`; query logic unchanged
- [X] T010 Change `src/Codium.Template.EntityFrameworkCore/Contexts/ApplicationDbContext.cs` to `IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>`; remove the now-inherited `Roles`, `UserRoles`, `Users` DbSets; call `base.OnModelCreating(builder)` before applying configurations (already the case)
- [X] T011 Rewrite `src/Codium.Template.EntityFrameworkCore/EntityConfigurations/UserConfiguration.cs`: keep table `ApplicationConsts.DbTablePrefix + "Users"` in `DbSchema`; keep `ApplyGlobalEntityConfigurations()`; unique index on `NormalizedEmail` filtered `"IsDeleted" = FALSE`; unique index on `NormalizedUserName` filtered the same way (replace Identity's default `UserNameIndex`); `Email` `HasMaxLength(UserConsts.EmailMaxLength)`; `PhoneNumber` `HasMaxLength(UserConsts.PhoneNumberMaxLength)`; `FirstName` `HasMaxLength(UserConsts.FirstNameMaxLength)` nullable; `LastName` `HasMaxLength(UserConsts.LastNameMaxLength)` nullable; `IsActive` default `true` required; drop the `PasswordHash` fixed-length rule only if it conflicts with Identity (keep `PasswordHashMaxLength`)
- [X] T012 [P] Rewrite `src/Codium.Template.EntityFrameworkCore/EntityConfigurations/RoleConfiguration.cs`: keep table `Roles`; unique index on `NormalizedName` filtered `"IsDeleted" = FALSE` (replace Identity's default `RoleNameIndex`); keep existing `Name` max length and `Description` rules; keep `ApplyGlobalEntityConfigurations()`
- [X] T013 [P] Rewrite `src/Codium.Template.EntityFrameworkCore/EntityConfigurations/UserRoleConfiguration.cs`: table `UserRoles`; `HasKey(x => new { x.UserId, x.RoleId })`; remove the surrogate-`Id` unique index and soft-delete filter; keep the two required cascade FKs to `User` and `Role`; apply creation-audit column config
- [X] T014 Register Identity in `src/Codium.Template.EntityFrameworkCore/ServiceCollectionExtensions.cs`: `services.AddIdentityCore<User>(o => { password/lockout/signin from UserConsts }).AddRoles<Role>().AddEntityFrameworkStores<ApplicationDbContext>()` (no cookie auth); use only the defaults for normalization (no custom `ILookupNormalizer`, per research R2); keep `IPasswordHasher<User>` registration in `src/Codium.Template.Application/ServiceCollectionExtensions.cs`
- [X] T015 Replace `NormalizeValue()` with Identity normalization in `src/Codium.Template.EntityFrameworkCore/Repositories/UserRepository.cs`: inject `ILookupNormalizer` and use `NormalizeEmail(email)`; remove the `Codium.Template.Domain.Shared.Extensions` using if unused
- [X] T016 [P] Replace `NormalizeValue()` with `ILookupNormalizer.NormalizeName(name)` in `src/Codium.Template.EntityFrameworkCore/Repositories/RoleRepository.cs` (both methods)
- [X] T017 Update `src/Codium.Template.Domain/DevelopmentDataSeederContributor.cs`: inject `ILookupNormalizer`; replace all three `NormalizeValue()` calls (roles at line ~112, admin role name ~123, admin email ~182); set `UserName = email`, `SecurityStamp = Guid.NewGuid().ToString()`, `NormalizedUserName`/`NormalizedEmail` via the normalizer, `NormalizedName` for roles via `NormalizeName`; create `UserRole` rows with composite key only (no `Id`)
- [X] T018 Build only the layers touched so far: `dotnet build src/Codium.Template.Domain` and `dotnet build src/Codium.Template.EntityFrameworkCore`; fix compile errors inside these two projects (seeder, repositories, configurations). The Application, HttpApi and Host projects are expected to fail until Phases 3-4 update the app services; do not fix them here

**Checkpoint**: Domain and EntityFrameworkCore projects compile on Identity types. Full-solution compilation is checked at the end of Phase 4 (T030).

## Phase 3: User Story 1 - Existing authentication flows keep working (Priority: P1) 🎯 MVP

**Goal**: Login, refresh, logout, change password, lockout, forced password change, unconfirmed/inactive denial behave exactly as before.

**Independent Test**: Run the auth calls against the seeded API and compare status codes and response shapes with the baseline captured in T004 (`tests/parity/baseline/`, contracts/api-parity.md). Note: Phase 3 tests only run after the full build in T030 because the app services and controllers must compile together.

- [X] T019 [US1] Update `src/Codium.Template.Application/Auth/AuthAppService.cs` for the new `User` shape: keep `PasswordHash` verification through `IPasswordHasher<User>` (line ~325), keep lockout/failed-count/forced-change logic on the inherited `LockoutEnd`/`LockoutEnabled`/`AccessFailedCount`; look users up by normalized email via `IUserRepository.FindByEmailAsync`; remove any assignment to properties that no longer exist
- [X] T020 [P] [US1] Update `src/Codium.Template.Application/Profiles/ProfileAppService.cs` (change password / own profile) for inherited properties; hash via `IPasswordHasher<User>`; keep `PasswordChangedTime` handling
- [X] T021 [P] [US1] Update `src/Codium.Template.Application/Users/CurrentUser.cs` and `src/Codium.Template.Application/AuthTokens/JwtTokenAppService.cs` only if they reference removed members; JWT claim names and values must not change
- [X] T022 [P] [US1] Update `src/Codium.Template.Application/ApplicationAutoMapperProfiles.cs` mappings for `User`/`Role` (inherited `Email`, `PhoneNumber`, `EmailConfirmed`, etc.) so `UserResponseDto` and `RoleResponseDto` are identical to before
- [X] T023 [US1] Integration tests in `tests/Codium.Template.IntegrationTests/AuthTests.cs` (compare against `tests/parity/baseline/` from T004): login success/response shape, refresh token, logout, lockout after `UserConsts.MaxFailedAccessAttempts` (5) failures, forced password change, unconfirmed email denial, inactive user denial, change password

**Checkpoint**: US1 verifiable independently; this is the MVP.

## Phase 4: User Story 2 - User management endpoints keep working (Priority: P1)

**Goal**: User, role, permission, profile, session endpoints keep routes, contracts and permission checks; role/user-role management runs on Identity roles.

**Independent Test**: Exercise every user/role/permission/session endpoint and compare with previous behavior.

- [X] T024 [US2] Update `src/Codium.Template.Application/Users/UserAppService.cs`: replace `NormalizeValue()` at lines ~100, ~119, ~150 with injected `ILookupNormalizer.NormalizeEmail`; on create set `UserName = request.Email` and `NormalizedUserName`/`NormalizedEmail` via the normalizer and `SecurityStamp`; keep `IsActive`, lockout defaults and forced-change flag; adapt `UserRoles` includes and `new UserRole { UserId, RoleId }` (no `Id`)
- [X] T025 [US2] In `UserAppService.SyncRolesAsync`/`AddToRoleAsync`/`RemoveFromRoleAsync` (`src/Codium.Template.Application/Users/UserAppService.cs`) make role-removal a hard delete of the composite `UserRole` row and keep the same conflict/not-found exceptions and localization keys
- [X] T026 [P] [US2] Update `src/Codium.Template.Application/Roles/RoleAppService.cs`: replace `NormalizeValue()` at lines ~75, ~93, ~117, ~140 with `ILookupNormalizer.NormalizeName`; keep `Description`, permission sync and duplicate-name checks
- [X] T027 [P] [US2] Leave `src/Codium.Template.Application/Permissions/PermissionAppService.cs` on `NormalizeValue()` (non-Identity entity); add a one-line code comment there explaining why it differs
- [X] T028 [P] [US2] Verify `src/Codium.Template.HttpApi/Controllers/v1/UserController.cs`, `RoleController.cs`, `ProfileController.cs`, `SessionController.cs` and `src/Codium.Template.HttpApi/Attributes/PermissionAuthorizeAttribute.cs` compile and require no contract change; fix only references to removed members
- [X] T029 [US2] Integration tests in `tests/Codium.Template.IntegrationTests/UserManagementTests.cs` and `RoleTests.cs` (compare against `tests/parity/baseline/` from T004): user list/get/create/update/delete, reset password, sync roles, role CRUD, sync role permissions, unauthorized access denied, case-insensitive email and role-name lookup, reuse of a soft-deleted user's email and a soft-deleted role's name

- [X] T030 [US2] Full solution build: `dotnet build Codium.sln`; fix any remaining compile errors in Application, HttpApi and Host caused by removed members, then run the US1 and US2 tests

**Checkpoint**: US1 and US2 both pass independently.

## Phase 5: User Story 3 - Fresh database works end to end (Priority: P2)

**Goal**: On an empty database the schema for the Identity model is created and seed data works.

**Independent Test**: Start the app against an empty PostgreSQL database and log in as the seeded admin.

- [X] T031 [US3] Generate the initial EF migration for the Identity-based model locally only to validate the mapping (schema for tests is created with `EnsureCreated()`, not this migration) (`dotnet ef migrations add InitialCreate -p src/Codium.Template.EntityFrameworkCore -s src/Codium.Template.HttpApi.Host`); inspect that tables `Users`, `Roles`, `UserRoles` keep prefix/schema, indexes are filtered by `IsDeleted`, and Identity claims/logins/tokens tables exist. Do not commit the migration (template ships none, research R6); remove it after inspection
- [X] T032 [US3] Verify `src/Codium.Template.Application/ApplicationSeedInitializer.cs` and `src/Codium.Template.EntityFrameworkCore/DbMigrationInitializer.cs` on a new database whose schema exists (created by `EnsureCreated()` in tests or by the consumer's own migration): seeder runs cleanly, and a second start causes no duplicate role/user/user-role errors. Note that `DbMigrationInitializer` applies nothing when no migrations exist; document this in quickstart.md (T034)
- [X] T033 [US3] Integration test in `tests/Codium.Template.IntegrationTests/FreshDatabaseTests.cs`: empty database (schema created by the fixture's `EnsureCreated()`) -> startup -> seeded admin can log in with seeded credentials and receives the seeded roles and permissions; audit fields and soft-delete are recorded when a user/role is created, updated and deleted

## Phase 6: Polish & Cross-Cutting

- [X] T034 [P] Update `specs/001-migrate-to-identity-user/quickstart.md` with the exact commands used in T031/T033 (migration generation and test run)
- [X] T035 Search for leftovers: `grep -rn "NormalizeValue()" src` must show only `PermissionAppService` and the extension definition; `grep -rn "FullAuditedEntity<Guid>" src/Codium.Template.Domain/{Users,Roles,UserRoles}` must be empty
- [X] T036 [P] Update `README`/docs (if present in repo root or `src/`) to state that the template uses ASP.NET Core Identity user/role stores and the accent-sensitivity difference (research R2)
- [X] T037 Run full `dotnet build Codium.sln` and `dotnet test`; walk through quickstart.md and record any deviation from contracts/api-parity.md

## Dependencies & Execution Order

- Phase 1 -> T004 (parity baseline, must run on unmodified code and be committed) -> Phase 2 -> (Phase 3, Phase 4 can proceed in parallel after Phase 2) -> Phase 5 -> Phase 6.
- Within Phase 2: T005/T006/T007 first (T004 must be finished before T005 starts) ([P] among T006, T007 after T005's audit pattern is set); T008-T009 after T007; T010 after T005-T007; T011-T013 after T010; T014 after T010; T015-T017 after T014; T018 last (builds only Domain and EntityFrameworkCore).
- The full solution build (T030) gates every test task: US1 tests (T023) can only run after T030 because Application, HttpApi and Host must compile together.
- US3 (T031-T033) needs Phases 2-4 complete because the seed and login path exercise them.
- Tests T023, T029, T033 depend on T003.

## Parallel Examples

- After T005: run T006 and T007 together (different files).
- After T010: T011, T012, T013 (different configuration files; T011 alone touches Users).
- Phase 3/4: T020, T021, T022 in parallel; T026, T027, T028 in parallel.

## Implementation Strategy

- **MVP**: Phases 1-3 (US1) code changes, verified after the full build in T030 - authentication is proven identical to the T004 baseline. Because the solution only compiles once US2 service edits are done, US1 and US2 are delivered together in practice.
- Then US2 (management endpoints and role handling), then US3 (fresh database verification), then polish.
- Keep `AuthAppService`/`UserAppService` logic as is (no `UserManager`/`SignInManager` rewrite in this phase, research R4).
