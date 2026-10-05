# Parity: account states at login (inactive, unconfirmed email, lockout, session limit)

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"admin@codium.com","password":"Pp123456*"}
HTTP 200
[Captures]
admin: jsonpath "$.data.accessToken"

# ---- inactive user
POST {{host}}/api/v1/users
Authorization: Bearer {{admin}}
Content-Type: application/json
{"email":"i-{{suffix}}@example.com","password":"Test1234*","confirmPassword":"Test1234*","emailConfirmed":true,"isActive":false}
HTTP 204

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"i-{{suffix}}@example.com","password":"Test1234*"}
HTTP *
[Captures]
status_inactive: status
body_inactive: body

# ---- unconfirmed email
POST {{host}}/api/v1/users
Authorization: Bearer {{admin}}
Content-Type: application/json
{"email":"c-{{suffix}}@example.com","password":"Test1234*","confirmPassword":"Test1234*","emailConfirmed":false,"isActive":true}
HTTP 204

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"c-{{suffix}}@example.com","password":"Test1234*"}
HTTP *
[Captures]
status_unconfirmed: status
body_unconfirmed: body

# ---- lockout after repeated failures
POST {{host}}/api/v1/users
Authorization: Bearer {{admin}}
Content-Type: application/json
{"email":"l-{{suffix}}@example.com","password":"Test1234*","confirmPassword":"Test1234*","emailConfirmed":true,"isActive":true}
HTTP 204

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"l-{{suffix}}@example.com","password":"Wrong1234*"}
HTTP *
[Captures]
status_fail_1: status

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"l-{{suffix}}@example.com","password":"Wrong1234*"}
HTTP *
[Captures]
status_fail_2: status

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"l-{{suffix}}@example.com","password":"Wrong1234*"}
HTTP *
[Captures]
status_fail_3: status

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"l-{{suffix}}@example.com","password":"Wrong1234*"}
HTTP *
[Captures]
status_fail_4: status

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"l-{{suffix}}@example.com","password":"Wrong1234*"}
HTTP *
[Captures]
status_fail_5: status
body_fail_5: body

# Correct password while locked out
POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"l-{{suffix}}@example.com","password":"Test1234*"}
HTTP *
[Captures]
status_locked_correct_password: status
body_locked_correct_password: body

GET {{host}}/api/v1/users/paged?search=l-{{suffix}}
Authorization: Bearer {{admin}}
HTTP 200
[Captures]
lid: jsonpath "$.data.data[0].id"
body_locked_user: body

# Admin unlock restores login
PATCH {{host}}/api/v1/users/{{lid}}/unlock
Authorization: Bearer {{admin}}
HTTP 204

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"l-{{suffix}}@example.com","password":"Test1234*"}
HTTP *
[Captures]
status_after_unlock: status
body_after_unlock: body

# ---- session limit per user (limit is 5; sixth login)
POST {{host}}/api/v1/users
Authorization: Bearer {{admin}}
Content-Type: application/json
{"email":"s-{{suffix}}@example.com","password":"Test1234*","confirmPassword":"Test1234*","emailConfirmed":true,"isActive":true}
HTTP 204

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"s-{{suffix}}@example.com","password":"Test1234*"}
HTTP 200
[Captures]
first_session_token: jsonpath "$.data.accessToken"

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"s-{{suffix}}@example.com","password":"Test1234*"}
HTTP 200

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"s-{{suffix}}@example.com","password":"Test1234*"}
HTTP 200

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"s-{{suffix}}@example.com","password":"Test1234*"}
HTTP 200

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"s-{{suffix}}@example.com","password":"Test1234*"}
HTTP 200

POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"s-{{suffix}}@example.com","password":"Test1234*"}
HTTP *
[Captures]
status_sixth_login: status
body_sixth_login: body

# Is the oldest session still valid after the sixth login?
GET {{host}}/api/v1/profile
Authorization: Bearer {{first_session_token}}
HTTP *
[Captures]
status_first_session_after_sixth_login: status
