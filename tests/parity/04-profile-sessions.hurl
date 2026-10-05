# Parity: profile, change password, sessions

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"admin@codium.com","password":"Pp123456*"}
HTTP 200
[Captures]
admin: jsonpath "$.data.accessToken"

# Create a regular user with a known password
POST {{host}}/api/v1/users
Authorization: Bearer {{admin}}
Content-Type: application/json
{"email":"p-{{suffix}}@example.com","password":"Test1234*","confirmPassword":"Test1234*","emailConfirmed":true,"firstName":"Pro","lastName":"File","isActive":true}
HTTP 204

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"p-{{suffix}}@example.com","password":"Test1234*"}
HTTP 200
[Captures]
tok: jsonpath "$.data.accessToken"

GET {{host}}/api/v1/profile
Authorization: Bearer {{tok}}
HTTP 200
[Captures]
body_profile: body

# Change password: wrong old password
POST {{host}}/api/v1/profile/change-password
Authorization: Bearer {{tok}}
Content-Type: application/json
{"oldPassword":"Nope12345*","newPassword":"Changed1234*","confirmNewPassword":"Changed1234*"}
HTTP *
[Captures]
status_change_wrong_old: status
body_change_wrong_old: body

# Change password: mismatch
POST {{host}}/api/v1/profile/change-password
Authorization: Bearer {{tok}}
Content-Type: application/json
{"oldPassword":"Test1234*","newPassword":"Changed1234*","confirmNewPassword":"Other1234*"}
HTTP 400
[Captures]
body_change_mismatch: body

# Change password succeeds
POST {{host}}/api/v1/profile/change-password
Authorization: Bearer {{tok}}
Content-Type: application/json
{"oldPassword":"Test1234*","newPassword":"Changed1234*","confirmNewPassword":"Changed1234*"}
HTTP *
[Captures]
status_change_ok: status
body_change_ok: body

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"p-{{suffix}}@example.com","password":"Changed1234*"}
HTTP 200
[Captures]
tok2: jsonpath "$.data.accessToken"
body_login_new_password: body

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"p-{{suffix}}@example.com","password":"Test1234*"}
HTTP *
[Captures]
status_login_old_password: status

# Sessions of the admin
GET {{host}}/api/v1/sessions/paged
Authorization: Bearer {{admin}}
HTTP *
[Captures]
status_sessions_paged: status
sid: jsonpath "$.data.data[0].id"
body_sessions_paged: body

DELETE {{host}}/api/v1/sessions/{{sid}}
Authorization: Bearer {{admin}}
HTTP *
[Captures]
status_session_delete: status
body_session_delete: body
