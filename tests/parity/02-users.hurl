# Parity: user management endpoints (admin)

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"admin@codium.com","password":"Pp123456*"}
HTTP 200
[Captures]
admin: jsonpath "$.data.accessToken"

# Create user
POST {{host}}/api/v1/users
Authorization: Bearer {{admin}}
Content-Type: application/json
{"email":"u-{{suffix}}@example.com","password":"Test1234*","confirmPassword":"Test1234*","emailConfirmed":true,"phoneNumber":"+905551112233","phoneNumberConfirmed":false,"twoFactorEnabled":false,"firstName":"Parity","lastName":"User","isActive":true}
HTTP 204

# Duplicate email (different casing) is rejected
POST {{host}}/api/v1/users
Authorization: Bearer {{admin}}
Content-Type: application/json
{"email":"U-{{suffix}}@Example.com","password":"Test1234*","confirmPassword":"Test1234*","emailConfirmed":true}
HTTP *
[Captures]
status_duplicate_email: status
body_duplicate_email: body

# Validation error on create
POST {{host}}/api/v1/users
Authorization: Bearer {{admin}}
Content-Type: application/json
{"email":"bad","password":"short","confirmPassword":"different"}
HTTP 400
[Captures]
body_create_validation: body

# List with search (case-insensitive)
GET {{host}}/api/v1/users/paged?search=U-{{suffix}}
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
uid: jsonpath "$.data.data[0].id"
body_paged_search: body
[Asserts]
jsonpath "$.data.data" count == 1

# List filtered by isActive
GET {{host}}/api/v1/users/paged?search=u-{{suffix}}&isActive=false
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
body_paged_inactive: body

# Options
GET {{host}}/api/v1/users/options?search=u-{{suffix}}
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
body_options: body

# Get by id
GET {{host}}/api/v1/users/{{uid}}
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
body_get: body

# Get unknown id
GET {{host}}/api/v1/users/00000000-0000-0000-0000-000000000001
Authorization: Bearer {{admin}}
HTTP *
[Captures]
status_get_unknown: status
body_get_unknown: body

# Update
PUT {{host}}/api/v1/users/{{uid}}
Authorization: Bearer {{admin}}
Content-Type: application/json
{"phoneNumber":"+905554445566","firstName":"Parity2","lastName":"User2","isActive":true,"emailConfirmed":true,"phoneNumberConfirmed":true,"twoFactorEnabled":false}
HTTP 204

GET {{host}}/api/v1/users/{{uid}}
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
body_get_after_update: body

# Toggles
PATCH {{host}}/api/v1/users/{{uid}}/email-confirmation
Authorization: Bearer {{admin}}
HTTP 204

PATCH {{host}}/api/v1/users/{{uid}}/phone-number-confirmation
Authorization: Bearer {{admin}}
HTTP 204

PATCH {{host}}/api/v1/users/{{uid}}/two-factor-enabled
Authorization: Bearer {{admin}}
HTTP 204

PATCH {{host}}/api/v1/users/{{uid}}/is-active
Authorization: Bearer {{admin}}
HTTP 204

GET {{host}}/api/v1/users/{{uid}}
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
body_get_after_toggles: body

# Restore active + email confirmed
PATCH {{host}}/api/v1/users/{{uid}}/is-active
Authorization: Bearer {{admin}}
HTTP 204

PATCH {{host}}/api/v1/users/{{uid}}/email-confirmation
Authorization: Bearer {{admin}}
HTTP 204

# Unlock (a user that is not locked)
PATCH {{host}}/api/v1/users/{{uid}}/unlock
Authorization: Bearer {{admin}}
HTTP 204

# Reset password then log in with the new one
PATCH {{host}}/api/v1/users/{{uid}}/reset-password
Authorization: Bearer {{admin}}
Content-Type: application/json
{"newPassword":"Reset1234*","confirmNewPassword":"Reset1234*"}
HTTP 204

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"u-{{suffix}}@example.com","password":"Reset1234*"}
HTTP *
[Captures]
status_login_after_reset: status
body_login_after_reset: body

# Old password no longer works
POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"u-{{suffix}}@example.com","password":"Test1234*"}
HTTP *
[Captures]
status_login_old_password: status
body_login_old_password: body

# Delete, then it is gone
DELETE {{host}}/api/v1/users/{{uid}}
Authorization: Bearer {{admin}}
HTTP 200

GET {{host}}/api/v1/users/{{uid}}
Authorization: Bearer {{admin}}
HTTP *
[Captures]
status_get_deleted: status
body_get_deleted: body

GET {{host}}/api/v1/users/paged?search=u-{{suffix}}
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
body_paged_after_delete: body

# Email of a deleted user can be reused
POST {{host}}/api/v1/users
Authorization: Bearer {{admin}}
Content-Type: application/json
{"email":"u-{{suffix}}@example.com","password":"Test1234*","confirmPassword":"Test1234*","emailConfirmed":true}
HTTP *
[Captures]
status_reuse_deleted_email: status

# Unauthenticated access
GET {{host}}/api/v1/users/paged
HTTP 401
