# JWT Authentication Implementation Guide

## Overview
JWT (JSON Web Token) authentication has been successfully implemented in your EXE201 Clothing Rental API without refresh tokens.

## What Was Implemented

### 1. NuGet Packages Added
- `Microsoft.AspNetCore.Authentication.JwtBearer` v8.0.0

### 2. Configuration in Program.cs
- **JWT Authentication** configured with token validation
- **Swagger UI** configured with JWT Bearer support
- **Authentication & Authorization** middleware added to the pipeline

### 3. Protected Endpoints
- All endpoints in `UserController` are now protected with `[Authorize]` attribute
- `AuthController` endpoints (register, login) remain public

## How to Use

### 1. Register a New User
**POST** `/api/Auth/register`

```json
{
  "email": "user@example.com",
  "password": "YourPassword123!",
  "fullName": "John Doe",
  "phoneNumber": "0123456789"
}
```

### 2. Login
**POST** `/api/Auth/login`

```json
{
"email": "user@example.com",
  "password": "YourPassword123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "userId": 1,
    "email": "user@example.com",
    "fullName": "John Doe",
    "phoneNumber": "0123456789",
    "roleId": 1,
    "roleName": "User",
    "isActive": true
  }
}
```

### 3. Access Protected Endpoints
To access protected endpoints, include the JWT token in the Authorization header:

**Header:**
```
Authorization: Bearer eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...
```

**Example with cURL:**
```bash
curl -X GET "https://localhost:7000/api/User" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

### 4. Using Swagger UI
1. Click the **Authorize** button (??) at the top right
2. Enter: `Bearer YOUR_JWT_TOKEN_HERE`
3. Click **Authorize**
4. Now you can test protected endpoints

## JWT Token Details

### Token Contains:
- **User ID** (NameIdentifier claim)
- **Email** (Email claim)
- **Full Name** (Name claim)
- **Role** (Role claim)
- **Expiration** (24 hours from creation)

### Token Settings (appsettings.json):
```json
{
  "AppSettings": {
    "Token": "MySuperDuperVerytotemoLongPassThatINotGonnaRememberInThisEntireSemaster",
    "Issuer": "MyIssuer",
    "Audience": "MyAudience"
  }
}
```

## Role-Based Authorization Examples

### Option 1: Specific Role Required
```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<ActionResult> DeleteUser(int id)
{
    // Only Admin role can access
}
```

### Option 2: Multiple Roles Allowed
```csharp
[Authorize(Roles = "Admin,Manager")]
[HttpPut("{id}")]
public async Task<ActionResult> UpdateUser(int id)
{
    // Admin or Manager can access
}
```

### Option 3: Public Endpoint (No Auth Required)
```csharp
[AllowAnonymous]
[HttpGet("public")]
public async Task<ActionResult> GetPublicData()
{
    // Anyone can access
}
```

### Option 4: Get Current User Info from Token
```csharp
[HttpGet("me")]
public ActionResult GetCurrentUser()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var email = User.FindFirst(ClaimTypes.Email)?.Value;
    var role = User.FindFirst(ClaimTypes.Role)?.Value;
    
    return Ok(new { userId, email, role });
}
```

## Security Considerations

### ? What's Implemented:
- Password hashing with `PasswordHasher<User>`
- JWT token signing with HMAC-SHA512
- Token expiration (24 hours)
- Issuer and Audience validation
- HTTPS enforcement

### ?? Recommendations:
1. **Store Secret Securely**: Move `AppSettings:Token` to environment variables or Azure Key Vault in production
2. **Use Strong Secret**: Minimum 256 bits (32 characters) for HMAC-SHA512
3. **HTTPS Only**: Always use HTTPS in production
4. **Token Expiration**: Consider shorter expiration times for sensitive operations
5. **Refresh Strategy**: Without refresh tokens, users must login again after 24 hours

## Testing the Implementation

### Test Flow:
1. **Register** a new user ? Get user details
2. **Login** with credentials ? Get JWT token
3. **Call protected endpoint** without token ? Get 401 Unauthorized
4. **Call protected endpoint** with token ? Get 200 OK with data
5. **Wait for token expiration** (24 hours) ? Get 401 Unauthorized

### HTTP Status Codes:
- **200 OK**: Successful request
- **400 Bad Request**: Invalid credentials or user already exists
- **401 Unauthorized**: Missing or invalid token
- **403 Forbidden**: Valid token but insufficient permissions (wrong role)
- **404 Not Found**: Resource not found

## Troubleshooting

### Issue: 401 Unauthorized
- Verify token is included in header: `Authorization: Bearer <token>`
- Check token hasn't expired (24 hour limit)
- Ensure token format is correct

### Issue: 403 Forbidden
- User doesn't have required role
- Check role requirements on endpoint

### Issue: Token validation fails
- Verify `AppSettings:Token`, `Issuer`, and `Audience` match in appsettings.json
- Check token was generated with same secret key

## Next Steps

### Optional Enhancements:
1. Add email verification
2. Implement password reset functionality
3. Add rate limiting to prevent brute force attacks
4. Implement account lockout after failed login attempts
5. Add custom authorization policies
6. Implement audit logging for authentication events
