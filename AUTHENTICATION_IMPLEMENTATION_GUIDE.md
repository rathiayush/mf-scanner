# Authentication & Authorization Implementation Guide

## Overview

This guide provides complete instructions for implementing the authentication and authorization system in the Mutual Fund Scanner application.

## Backend Setup

### 1. Install Required NuGet Packages

```bash
# Navigate to API project
cd backend/MutualFundScanner.API
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

# Navigate to Application project
cd ../MutualFundScanner.Application
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package BCrypt.Net-Next
```

### 2. Create Database Migration

```bash
cd backend/MutualFundScanner.API
dotnet ef migrations add AddUserAuthentication
dotnet ef database update
```

### 3. Create Default Admin User

After running the migration, you need to create the first admin user. You can do this in two ways:

#### Option A: Using SQL Script
```sql
-- Generate BCrypt hash for "Admin@123" using online tool or code
-- Example hash: $2a$11$YourGeneratedHashHere

INSERT INTO Users (Username, Email, PasswordHash, Role, IsActive, CreatedDate)
VALUES ('admin', 'admin@mutualfundscanner.com', '$2a$11$YourGeneratedHashHere', 1, 1, GETUTCDATE());
```

#### Option B: Using C# Console App (Recommended)
Create a simple console app to generate the hash:

```csharp
using BCrypt.Net;

var password = "Admin@123";
var hash = BCrypt.HashPassword(password);
Console.WriteLine($"Hashed Password: {hash}");
```

Then use the generated hash in the SQL INSERT statement.

### 4. Update JWT Secret Key

**IMPORTANT**: Change the JWT secret key in `appsettings.json` to a strong, unique value:

```json
{
  "JwtSettings": {
    "SecretKey": "YOUR_STRONG_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG",
    "Issuer": "MutualFundScanner",
    "Audience": "MutualFundScannerUsers",
    "ExpirationHours": 24
  }
}
```

## Frontend Setup

### 1. Update App.tsx

Wrap your app with `AuthProvider` and add protected routes:

```tsx
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import Login from './pages/Login';
import UserManagement from './pages/UserManagement';
// ... other imports

function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          <Route path="/login" element={<Login />} />
          
          <Route path="/" element={
            <ProtectedRoute>
              <Layout>
                {/* Your main app routes */}
              </Layout>
            </ProtectedRoute>
          } />
          
          <Route path="/user-management" element={
            <ProtectedRoute requireAdmin>
              <Layout>
                <UserManagement />
              </Layout>
            </ProtectedRoute>
          } />
          
          {/* Add other protected routes */}
        </Routes>
      </Router>
    </AuthProvider>
  );
}
```

### 2. Update Layout Component

Add user info and logout button to your layout:

```tsx
import { useAuth } from '../contexts/AuthContext';
import { Button, Dropdown, Avatar } from 'antd';
import { UserOutlined, LogoutOutlined } from '@ant-design/icons';

const Layout = () => {
  const { user, logout, isAdmin } = useAuth();

  const userMenuItems = [
    {
      key: 'profile',
      label: user?.email,
      disabled: true,
    },
    {
      key: 'logout',
      label: 'Logout',
      icon: <LogoutOutlined />,
      onClick: logout,
    },
  ];

  return (
    <AntLayout>
      <Header>
        {/* Your header content */}
        <Dropdown menu={{ items: userMenuItems }} placement="bottomRight">
          <Button type="text" icon={<Avatar icon={<UserOutlined />} />}>
            {user?.username}
          </Button>
        </Dropdown>
      </Header>
      {/* Rest of layout */}
    </AntLayout>
  );
};
```

### 3. Add User Management to Menu (Admin Only)

```tsx
import { useAuth } from '../contexts/AuthContext';

const MenuComponent = () => {
  const { isAdmin } = useAuth();

  const menuItems = [
    // ... other menu items
    isAdmin && {
      key: 'user-management',
      icon: <TeamOutlined />,
      label: 'User Management',
      onClick: () => navigate('/user-management'),
    },
  ].filter(Boolean);

  return <Menu items={menuItems} />;
};
```

## API Endpoints

### Public Endpoints
- `POST /api/auth/login` - Login and get JWT token

### Authenticated Endpoints
- `GET /api/auth/me` - Get current user info
- `POST /api/auth/change-password` - Change own password

### Admin-Only Endpoints
- `GET /api/usermanagement` - Get all users
- `POST /api/usermanagement` - Create new user
- `PUT /api/usermanagement/{id}` - Update user
- `DELETE /api/usermanagement/{id}` - Delete user
- `POST /api/usermanagement/{id}/activate` - Activate user
- `POST /api/usermanagement/{id}/deactivate` - Deactivate user
- `POST /api/usermanagement/reset-password` - Reset user password

## Testing the Implementation

### 1. Test Login
```bash
curl -X POST http://localhost:5247/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}'
```

Expected response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@mutualfundscanner.com",
    "role": "Admin",
    "isActive": true
  },
  "expiresAt": "2024-11-02T12:00:00Z"
}
```

### 2. Test Protected Endpoint
```bash
curl -X GET http://localhost:5247/api/auth/me \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### 3. Test User Creation (Admin)
```bash
curl -X POST http://localhost:5247/api/usermanagement \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "serviceuser1",
    "email": "service@example.com",
    "password": "Password123",
    "role": "ServiceUser"
  }'
```

## Security Best Practices

### Production Deployment

1. **Use HTTPS**: Always use HTTPS in production
2. **Strong Secret Key**: Use a cryptographically secure random key (at least 32 characters)
3. **Environment Variables**: Store sensitive config in environment variables
4. **Token Expiration**: Consider shorter token expiration times
5. **Refresh Tokens**: Implement refresh token mechanism for better security
6. **Rate Limiting**: Add rate limiting to login endpoint
7. **Account Lockout**: Implement account lockout after failed attempts
8. **Password Policy**: Enforce strong password requirements
9. **Audit Logging**: Log all authentication and authorization events
10. **CORS**: Configure CORS properly for production domains

### Example Production Configuration

```json
{
  "JwtSettings": {
    "SecretKey": "${JWT_SECRET_KEY}",
    "Issuer": "${JWT_ISSUER}",
    "Audience": "${JWT_AUDIENCE}",
    "ExpirationHours": 8
  }
}
```

Set environment variables:
```bash
export JWT_SECRET_KEY="your-production-secret-key-here"
export JWT_ISSUER="https://your-domain.com"
export JWT_AUDIENCE="https://your-domain.com"
```

## User Roles

### Admin
- Full access to all features
- Can create, update, delete users
- Can assign roles
- Can reset passwords
- Can activate/deactivate users

### ServiceUser
- Access to application features
- Cannot manage users
- Can change own password
- Can view own profile

## Troubleshooting

### Common Issues

1. **401 Unauthorized on all requests**
   - Check if token is being sent in Authorization header
   - Verify token hasn't expired
   - Check JWT secret key matches between token generation and validation

2. **Cannot login**
   - Verify user exists in database
   - Check password hash is correct
   - Ensure user is active

3. **NuGet package errors**
   - Ensure all packages are installed correctly
   - Try `dotnet restore`
   - Check package versions compatibility

4. **Migration errors**
   - Ensure connection string is correct
   - Check SQL Server is running
   - Verify EF Core tools are installed

## Next Steps

1. Implement password reset via email
2. Add two-factor authentication
3. Implement refresh tokens
4. Add audit logging for user actions
5. Implement session management
6. Add password expiration policy
7. Implement IP whitelisting for admin accounts

## Support

For issues or questions, refer to:
- Backend code: `backend/MutualFundScanner.Application/Services/AuthService.cs`
- Frontend code: `frontend/src/contexts/AuthContext.tsx`
- API documentation: Swagger UI at `http://localhost:5247/swagger`
