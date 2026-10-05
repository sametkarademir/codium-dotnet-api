# Feature Specification: Migrate Custom User to Microsoft Identity User

**Feature Branch**: `001-migrate-to-identity-user`

**Created**: 2026-09-29

**Status**: Draft

**Input**: User description: "Template projesindeki mevcut custom User yapısı yerine Microsoft Identity (ASP.NET Core Identity) User üzerinden çalışmaya geç. Mevcut yapı, endpoint'ler ve davranışlar bozulmadan korunmalı. İlk aşama sadece User altyapısının Identity'ye taşınmasını kapsar."

## Clarifications

### Session 2026-09-29

- Q: Bu aşamada rol yapısı (Role, UserRole, RolePermission) mevcut haliyle mi kalsın, yoksa Microsoft Identity rollerine mi taşınsın? → A: Roller de Identity rollerine taşınır; kullanıcı-rol ilişkisi Identity tarafından yönetilir, RolePermission Identity rolüne bağlanır.
- Note: Template'te mevcut bir veritabanı/migration yok; veri dönüştürme kapsam dışı, yalnızca yeni veritabanı desteklenir.
- Note: User/Role normalizasyonu için `NormalizeValue()` kullanılmaz; Identity'nin kendi normalizasyonuna güvenilir.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Existing authentication flows keep working (Priority: P1)

An API consumer (or end user) logs in, refreshes a session, logs out and changes a password exactly as before. The only difference is that the account data is now managed by the Microsoft Identity user infrastructure instead of the template's custom user model.

**Why this priority**: The template is used as a starting point for other projects; the core promise of this phase is "nothing breaks". Authentication is the most critical flow that depends on the user model.

**Independent Test**: Run the existing login, refresh-token, logout and change-password calls against the migrated API and confirm request/response shapes and outcomes are identical to the pre-migration behavior.

**Acceptance Scenarios**:

1. **Given** an active, email-confirmed user with valid credentials, **When** the user logs in, **Then** the API returns tokens in the same response shape as before.
2. **Given** a user with a valid refresh token, **When** the user requests a token refresh, **Then** a new token pair is issued as before.
3. **Given** a user who fails login repeatedly up to the configured limit, **When** the next attempt is made, **Then** the account is locked out for the configured duration as before.
4. **Given** a user whose password must be changed on next login, **When** they log in, **Then** the same forced-change behavior applies as before.
5. **Given** an unconfirmed email or inactive account, **When** login is attempted, **Then** access is denied with the same error outcome as before.

---

### User Story 2 - User management endpoints keep working (Priority: P1)

An administrator lists, creates, updates, deletes users, resets passwords and assigns roles through the existing user endpoints, with unchanged routes, request/response contracts and permission checks.

**Why this priority**: These endpoints are the public contract of the template; consumers of the template must not need to change their clients.

**Independent Test**: Exercise every existing user, profile, role, permission and session endpoint and compare responses (status codes, fields, error format) with the previous template behavior.

**Acceptance Scenarios**:

1. **Given** an authorized administrator, **When** they create, update, list or delete users, **Then** results match the pre-migration behavior including the standard `Result<T>` response envelope.
2. **Given** an administrator, **When** they sync a user's roles or reset a user's password, **Then** the change takes effect and is reflected in subsequent logins and permission checks.
3. **Given** a user without the required permission, **When** they call a protected endpoint, **Then** access is denied as before.
4. **Given** a signed-in user, **When** they read or update their own profile, **Then** behavior is unchanged.

---

### User Story 3 - Fresh database works end to end (Priority: P2)

A team starts a new project from the template. On a brand-new database, the schema is created for the Identity-based model and the seeded development data (users, roles, permissions) works out of the box.

**Why this priority**: The template ships without an existing database or migrations, so a clean first run is the only setup path to prove; it is secondary to authentication and management behavior being correct.

**Independent Test**: Create a new database from the template, run the application, and confirm the seeded user can log in and holds the seeded roles and permissions.

**Acceptance Scenarios**:

1. **Given** an empty database, **When** the application starts, **Then** the schema is created and development data is seeded without errors.
2. **Given** the seeded user, **When** they log in with the seeded credentials, **Then** login succeeds and the expected roles and permissions apply.
3. **Given** seeded roles and role-permission links, **When** roles are listed or synced, **Then** results match the previous template behavior.
4. **Given** audit and soft-delete rules, **When** a user or role is created, updated or deleted, **Then** audit metadata and soft-delete state are recorded as before.

---


### Edge Cases

- Soft-deleted users must remain excluded from login and lists.
- Emails and role names that differ only by letter case must be treated as the same value for lookup and uniqueness, using the Identity infrastructure's normalization.
- A soft-deleted user's email or a soft-deleted role's name must be reusable by a new record, as before.
- Sessions and refresh tokens must stay linked to the user when roles change or the user is deleted, as before.
- Role name and role-permission changes must be reflected in the next login and permission check.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST use the Microsoft Identity user model as the single source of truth for user accounts, replacing the template's custom user entity.
- **FR-002**: All existing HTTP endpoints, routes, request/response contracts and the `Result<T>` response envelope MUST remain unchanged.
- **FR-003**: Login, token refresh, logout, change password, reset password, lockout and forced-password-change behavior MUST remain functionally equivalent.
- **FR-004**: Existing password policy, lockout policy and sign-in requirements (confirmed email, session limit per user) MUST keep the same configured values and effect.
- **FR-005**: User properties currently available (email, phone number, first name, last name, active flag, email/phone confirmation, two-factor flag, lockout state, password-change tracking) MUST remain available with the same meaning.
- **FR-006**: Roles MUST be managed by the Microsoft Identity role infrastructure (roles and user-role assignments), replacing the template's custom role and user-role entities. Role-permission assignment and authorization checks MUST behave the same as before.
- **FR-007**: Sessions and refresh tokens MUST remain linked to users and keep their current lifecycle rules.
- **FR-008**: Audit tracking (created/modified/deleted metadata) and soft-delete behavior for users MUST be preserved.
- **FR-009**: The database schema and development seed data MUST be generated for the Identity-based model on a new database; no conversion of existing databases is provided or required.
- **FR-010**: Normalization of emails, usernames and role names for users and roles MUST rely on the Identity infrastructure's own normalization; the template's custom normalization helper MUST NOT be used for user or role values.
- **FR-011**: This phase MUST NOT introduce new user-facing capabilities (e.g., external logins, new token flows, new endpoints); it is limited to moving the user infrastructure to Identity.
- **FR-012**: Existing automated tests and functional test plans MUST continue to pass, and new tests MUST cover the migrated user behaviors.

### Key Entities

- **User**: An account able to sign in. Holds identity (email, phone, names), credential state, lockout state, activity/confirmation flags, and audit metadata. Related to roles, sessions and refresh tokens.
- **Role**: A named group of permissions assignable to users, managed by the Identity role infrastructure; public behavior unchanged.
- **User-Role assignment**: Link between a user and a role, managed by the Identity infrastructure.
- **Role Permission**: Link between a role and a permission; now attached to the Identity role.
- **Session**: An active login context belonging to a user, subject to a per-user limit.
- **Refresh Token**: A renewable credential belonging to a user and session.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of existing endpoints return the same status codes and response shapes for the same inputs as before the migration.
- **SC-002**: On a new database, 100% of seeded users can sign in with their seeded credentials on first run.
- **SC-003**: 100% of seeded roles, user-role assignments and role-permission links yield the same effective permissions as in the previous template.
- **SC-004**: All previously passing automated tests pass with no test-contract changes, other than those directly tied to the user model.
- **SC-005**: A new project created from the template starts on an empty database with a single schema-creation step and no manual data fixes.

## Assumptions

- Users and roles move to Microsoft Identity in this phase; permissions, sessions and refresh tokens keep their current behavior and public contracts.
- Custom JWT-based token issuance and refresh-token handling stay as they are today.
- Existing custom fields (first name, last name, active flag, forced password change, password-changed time) are retained as extensions to the Identity user.
- Identifier type for users and roles stays the same as today.
- Any new Identity features (external logins, authenticator apps, etc.) are out of scope for this phase and may be addressed later.
- The template ships without a database or committed migrations, so no existing data needs to be converted; only new databases are supported.
