# API parity checks

Hurl files that exercise the auth, user, role, permission, profile and session endpoints, plus a masking
script that turns `hurl --json` output into stable snapshots.

- `baseline/` - snapshots captured on the code **before** the Identity migration (tasks.md T004).
- `run.sh <out-dir> [host]` - runs every `NN-*.hurl` file and writes masked snapshots to `<out-dir>`.
- `mask.py` - masks guids, JWTs, timestamps, refresh tokens and the per-run random suffix; sorts lists of objects.

## Running

1. Start a throwaway PostgreSQL database and a fresh schema (the template ships no migrations; generate one
   locally with `dotnet ef migrations add Parity -p src/Codium.Template.EntityFrameworkCore -s src/Codium.Template.HttpApi.Host`
   and delete it afterwards, or create the schema with `EnsureCreated()`).
2. Start the Host against it with a large rate limit (keeping rate limiting enabled; disabling it breaks startup):

   ```sh
   export ConnectionStrings__Default="Host=localhost;Port=15432;Database=parity_db;User ID=postgres;Password=parity;"
   export ASPNETCORE_URLS=http://localhost:5100 ASPNETCORE_ENVIRONMENT=Development Hangfire__CronJobsEnabled=false
   export RateLimiting__GlobalPolicy__PermitLimit=1000000 RateLimiting__ApiPolicy__PermitLimit=1000000 RateLimiting__AuthPolicy__PermitLimit=1000000
   dotnet run --project src/Codium.Template.HttpApi.Host --no-launch-profile
   ```

3. The database must be **freshly created** for every run (list counts and session data depend on it).
4. `tests/parity/run.sh /tmp/current && diff -r tests/parity/baseline /tmp/current`

A difference in a snapshot is a behavior change; decide whether it is intended before accepting it.

## Accepted differences from the pre-Identity template

`baseline/` now holds the behavior after the Identity migration. These differences from the original template were reviewed
and accepted when the baseline was refreshed:

1. **Weak password messages:** the character rules of the password policy are validated in the request
   (`MustSatisfyPasswordPolicy`), so `POST /users` with a weak password lists the digit, uppercase and special-character
   errors under `Password` (and `NewPassword` for reset/change) in addition to the length error.
2. **`accessFailedCount` after lockout:** Identity resets the counter to 0 when it locks the account; the template left it at the limit.
3. **New users must change their password:** `POST /users` creates the user with `shouldChangePasswordOnNextLogin = true`.
4. **`PUT /users/{id}` scope:** it updates phone number, first name, last name and active flag only. `emailConfirmed`,
   `phoneNumberConfirmed` and `twoFactorEnabled` in the body are ignored; they change through the dedicated toggle endpoints.
5. **Removed endpoint:** `PATCH /users/{id}/lock` no longer exists (its Hurl steps were removed and the baseline was recaptured
   from the original code without them).
6. **Other known behavior notes:** a wrong password for an unconfirmed-email user answers 403 (Identity checks confirmation first),
   and locking/unlocking follows Identity for users with lockout disabled (see `quickstart.md`).


## Where this fits

The parity files check the HTTP contract against the pre-Identity behavior. Pure logic is covered by the fast unit tests in
`tests/Codium.Template.UnitTests`, and database behavior (seeding, audit, soft delete, indexes) by
`tests/Codium.Template.IntegrationTests`. See `specs/001-migrate-to-identity-user/quickstart.md` for the overview.
