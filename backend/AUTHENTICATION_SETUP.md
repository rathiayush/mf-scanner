# Authentication System Setup Guide

## Required NuGet Packages

Add the following NuGet packages to the respective projects:

### MutualFundScanner.API
```bash
cd MutualFundScanner.API
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### MutualFundScanner.Application
```bash
cd MutualFundScanner.Application
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package BCrypt.Net-Next
```

## Database Migration

Create and apply the migration for the User table:

```bash
cd MutualFundScanner.API
dotnet ef migrations add AddUserAuthentication
dotnet ef database update
```

## Create Default Admin User

After running the migration, execute this SQL script to create the default admin user:

```sql
-- Default password is "Admin@123" (hashed with BCrypt)
INSERT INTO Users (Username, Email, PasswordHash, Role, IsActive, CreatedDate)
VALUES ('admin', 'admin@mutualfundscanner.com', '$2a$11$YourHashedPasswordHere', 1, 1, GETUTCDATE());
```

**Note:** You'll need to hash the password using BCrypt before inserting. You can use the API's create user endpoint after logging in with the first admin, or use an online BCrypt generator.

## Testing the Authentication

1. **Login**:
   ```
   POST /api/auth/login
   {
     "username": "admin",
     "password": "Admin@123"
   }
   ```

2. **Get Current User**:
   ```
   GET /api/auth/me
   Authorization: Bearer {token}
   ```

3. **Create User (Admin only)**:
   ```
   POST /api/usermanagement
   Authorization: Bearer {token}
   {
     "username": "serviceuser1",
     "email": "service@example.com",
     "password": "Password123",
     "role": "ServiceUser"
   }
   ```

## Security Notes

1. **Change the JWT Secret Key** in `appsettings.json` to a strong, unique value in production
2. **Use HTTPS** in production environments
3. **Store sensitive configuration** in environment variables or Azure Key Vault
4. **Implement rate limiting** for login endpoints to prevent brute force attacks
5. **Add password complexity requirements** as needed
6. **Implement account lockout** after failed login attempts

## User Roles

- **Admin**: Full access to all features including user management
- **ServiceUser**: Access to application features but cannot manage users

## API Endpoints

### Authentication (Public)
- `POST /api/auth/login` - Login and get JWT token
- `GET /api/auth/me` - Get current user info (requires authentication)
- `POST /api/auth/change-password` - Change own password (requires authentication)

### User Management (Admin Only)
- `GET /api/usermanagement` - Get all users
- `GET /api/usermanagement/{id}` - Get user by ID
- `POST /api/usermanagement` - Create new user
- `PUT /api/usermanagement/{id}` - Update user
- `DELETE /api/usermanagement/{id}` - Delete user
- `POST /api/usermanagement/{id}/activate` - Activate user
- `POST /api/usermanagement/{id}/deactivate` - Deactivate user
- `POST /api/usermanagement/reset-password` - Reset user password
