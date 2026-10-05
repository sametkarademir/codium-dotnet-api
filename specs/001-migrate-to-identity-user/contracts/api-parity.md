# Contract: API Parity

No endpoint, route, request DTO, response DTO, status code or `Result<T>` envelope changes. Controllers under `src/Codium.Template.HttpApi/Controllers/v1` stay as-is:

- `AuthController` - login, refresh token, logout
- `UserController` - list/get/create/update/delete, reset password, sync roles
- `ProfileController` - own profile, change password
- `RoleController` - CRUD, sync role permissions
- `PermissionController`, `SessionController` - unchanged

Baseline: responses from the unmodified code are captured before any change (tasks.md T004) under `tests/parity/baseline/`, with volatile fields (ids, tokens, timestamps) masked.

Parity rule: for identical inputs against pre- and post-migration builds, status code, body shape and error format must match (SC-001). JWT claims (user id, roles, permissions, tenant) keep the same names and values.

Only non-contract internal change: role assignment removal is a hard delete of the join row.
