# Parity: authentication flows (login, refresh, logout, validation, unauthorized)
# Variables: host (default http://localhost:5100), suffix (random per run)

# 1. Admin login succeeds
POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"admin@codium.com","password":"Pp123456*"}
HTTP 200
[Captures]
access: jsonpath "$.data.accessToken"
refresh: jsonpath "$.data.refreshToken"
body_login: body
[Asserts]
jsonpath "$.success" == true
jsonpath "$.data.accessToken" isString
jsonpath "$.data.refreshToken" isString
jsonpath "$.data.expiryTime" isInteger

# 2. Wrong password (valid format) is rejected
POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"admin@codium.com","password":"Wrong1234*"}
HTTP *
[Captures]
status_wrong_password: status
body_wrong_password: body

# 3. Unknown email is rejected
POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"nobody-{{suffix}}@example.com","password":"Wrong1234*"}
HTTP *
[Captures]
status_unknown_email: status
body_unknown_email: body

# 4. Validation errors are reported
POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"bad","password":"x"}
HTTP 400
[Captures]
body_validation: body

# 5. Email casing does not matter
POST {{host}}/api/v1/auth/login
Content-Type: application/json
{"email":"ADMIN@Codium.com","password":"Pp123456*"}
HTTP *
[Captures]
status_email_case: status
body_email_case: body

# 6. Refresh token issues a new pair
POST {{host}}/api/v1/auth/refresh-token
Content-Type: application/json
{"refreshToken":"{{refresh}}"}
HTTP 200
[Captures]
access2: jsonpath "$.data.accessToken"
refresh2: jsonpath "$.data.refreshToken"
body_refresh: body
[Asserts]
jsonpath "$.success" == true

# 7. Reusing the old refresh token is rejected
POST {{host}}/api/v1/auth/refresh-token
Content-Type: application/json
{"refreshToken":"{{refresh}}"}
HTTP *
[Captures]
status_refresh_reuse: status
body_refresh_reuse: body

# 8. Protected endpoint without token
GET {{host}}/api/v1/profile
HTTP 401

# 9. Logout
POST {{host}}/api/v1/auth/logout
Authorization: Bearer {{access2}}
HTTP 204

# 10. Token is not usable after logout
GET {{host}}/api/v1/profile
Authorization: Bearer {{access2}}
HTTP *
[Captures]
status_after_logout: status
