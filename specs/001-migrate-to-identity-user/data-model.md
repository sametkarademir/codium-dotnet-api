# Data Model: Identity Migration

Key type stays `Guid`. Table names keep the current prefix/schema.

## User (table `Users`)

Base: `IdentityUser<Guid>` + `IFullAuditedObject`, `IEntity<Guid>`.

| Field | Source | Notes |
|-------|--------|-------|
| Id, Email, NormalizedEmail, EmailConfirmed | Identity | normalized by Identity's normalizer; filtered unique index on NormalizedEmail where not deleted |
| UserName, NormalizedUserName | Identity (new) | set to email; normalized by Identity; unique among non-deleted |
| PasswordHash, SecurityStamp | Identity | existing hashes stay valid |
| PhoneNumber, PhoneNumberConfirmed | Identity | unchanged |
| LockoutEnd, LockoutEnabled, AccessFailedCount | Identity | unchanged; policy from `UserConsts` |
| TwoFactorEnabled | Identity | unchanged |
| ConcurrencyStamp | Identity | `string?`; `IConcurrencyStamp` implemented explicitly |
| FirstName, LastName, IsActive | custom | kept |
| ShouldChangePasswordOnNextLogin, PasswordChangedTime | custom | kept |
| CreationTime, CreatorId, LastModificationTime, LastModifierId, IsDeleted, DeleterId, DeletionTime | custom audit | kept via interfaces |
| Navigations: UserRoles, RefreshTokens, Sessions | custom | kept |

## Role (table `Roles`)

Base: `IdentityRole<Guid>` + audit/soft-delete interfaces. `Name`, `NormalizedName`, `ConcurrencyStamp` inherited; `Description` kept; navigations `UserRoles`, `RolePermissions` kept. Unique NormalizedName filtered by `IsDeleted = false`.

## UserRole (table `UserRoles`)

Base: `IdentityUserRole<Guid>` + `ICreationAuditedObject`. Composite PK (`UserId`,`RoleId`); no `Id`, no soft-delete. FKs to `Users`/`Roles` cascade as today.

## Unchanged entities

`RolePermission` (FK -> Role), `Permission`, `Session` (FK -> User), `RefreshToken` (FK -> User, Session), audit/log tables.

## New (unused in phase 1)

`UserClaims`, `RoleClaims`, `UserLogins`, `UserTokens` created by Identity mapping; no application code uses them yet.

## State transitions

Unchanged: user active/inactive, locked/unlocked, forced password change; refresh token used/revoked; session revoked.
