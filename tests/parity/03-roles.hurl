# Parity: role and permission endpoints, role assignment to users

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"admin@codium.com","password":"Pp123456*"}
HTTP 200
[Captures]
admin: jsonpath "$.data.accessToken"

# Permissions list
GET {{host}}/api/v1/permissions/paged?perPage=100
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
body_permissions_paged: body

GET {{host}}/api/v1/permissions/options
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
perm_id: jsonpath "$.data.data[0].id"
perm_id2: jsonpath "$.data.data[1].id"
body_permissions_options: body

# Create role
POST {{host}}/api/v1/roles
Authorization: Bearer {{admin}}
Content-Type: application/json
{"name":"Parity-{{suffix}}","description":"parity role"}
HTTP 204

# Duplicate role (different casing)
POST {{host}}/api/v1/roles
Authorization: Bearer {{admin}}
Content-Type: application/json
{"name":"PARITY-{{suffix}}","description":"dup"}
HTTP *
[Captures]
status_duplicate_role: status
body_duplicate_role: body

GET {{host}}/api/v1/roles/paged?search=parity-{{suffix}}
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
rid: jsonpath "$.data.data[0].id"
body_roles_paged: body
[Asserts]
jsonpath "$.data.data" count == 1

GET {{host}}/api/v1/roles/options?search=parity-{{suffix}}
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
body_roles_options: body

GET {{host}}/api/v1/roles/{{rid}}
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
body_role_get: body

PUT {{host}}/api/v1/roles/{{rid}}
Authorization: Bearer {{admin}}
Content-Type: application/json
{"name":"Parity2-{{suffix}}","description":"updated"}
HTTP 204

GET {{host}}/api/v1/roles/{{rid}}
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
body_role_get_after_update: body

# Sync permissions
PATCH {{host}}/api/v1/roles/{{rid}}/sync-permissions
Authorization: Bearer {{admin}}
Content-Type: application/json
{"permissionIds":["{{perm_id}}","{{perm_id2}}"]}
HTTP 204

GET {{host}}/api/v1/roles/{{rid}}
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
body_role_get_after_sync: body

# Create a user and assign the role
POST {{host}}/api/v1/users
Authorization: Bearer {{admin}}
Content-Type: application/json
{"email":"r-{{suffix}}@example.com","password":"Test1234*","confirmPassword":"Test1234*","emailConfirmed":true,"isActive":true}
HTTP 204

GET {{host}}/api/v1/users/paged?search=r-{{suffix}}
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
uid: jsonpath "$.data.data[0].id"

PATCH {{host}}/api/v1/users/{{uid}}/sync-roles
Authorization: Bearer {{admin}}
Content-Type: application/json
{"roleIds":["{{rid}}"]}
HTTP 204

GET {{host}}/api/v1/users/{{uid}}
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
body_user_with_role: body

# The new user logs in and sees the role and its two permissions in its token
POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"r-{{suffix}}@example.com","password":"Test1234*"}
HTTP 200
[Captures]
user_token: jsonpath "$.data.accessToken"

# Permission enforcement: a user without User.View cannot list users
GET {{host}}/api/v1/users/paged
Authorization: Bearer {{user_token}}
HTTP *
[Captures]
status_forbidden: status
body_forbidden: body

# Removing all roles
PATCH {{host}}/api/v1/users/{{uid}}/sync-roles
Authorization: Bearer {{admin}}
Content-Type: application/json
{"roleIds":[]}
HTTP 204

GET {{host}}/api/v1/users/{{uid}}
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
body_user_without_role: body

# Delete role; name becomes reusable
DELETE {{host}}/api/v1/roles/{{rid}}
Authorization: Bearer {{admin}}
HTTP 204

GET {{host}}/api/v1/roles/{{rid}}
Authorization: Bearer {{admin}}
HTTP *
[Captures]
status_role_get_deleted: status
body_role_get_deleted: body

POST {{host}}/api/v1/roles
Authorization: Bearer {{admin}}
Content-Type: application/json
{"name":"Parity2-{{suffix}}","description":"reused"}
HTTP *
[Captures]
status_reuse_deleted_role: status

# Unauthenticated
GET {{host}}/api/v1/roles/paged
HTTP 401
