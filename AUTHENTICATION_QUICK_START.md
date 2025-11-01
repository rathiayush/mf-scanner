# Authentication System - Quick Start Guide

## 🚀 Quick Setup (5 Steps)

### Step 1: Install NuGet Packages
```bash
cd backend/MutualFundScanner.API
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

cd ../MutualFundScanner.Application
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package BCrypt.Net-Next
```

### Step 2: Run Database Migration
```bash
cd backend/MutualFundScanner.API
dotnet ef migrations add AddUserAuthentication
dotnet ef database update
```

### Step 3: Create Default Admin User

**Generate Password Hash:**
```csharp
// Run this in a C# console or use online BCrypt generator
using BCrypt.Net;
var hash = BCrypt.HashPassword("Admin@123");
Console.WriteLine(hash);
```

**Insert Admin User:**
```sql
INSERT INTO Users (Username, Email, PasswordHash, Role, IsActive, CreatedDate)
VALUES ('admin', 'admin@mutualfundscanner.com', 
        '$2a$11$[YOUR_GENERATED_HASH]', 1, 1, GETUTCDATE());
```

### Step 4: Update App.tsx (Frontend)

```tsx
import { AuthProvider } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import Login from './pages/Login';
import UserManagement from './pages/UserManagement';

function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          <Route path="/login" element={<Login />} />
          
          <Route path="/*" element={
            <ProtectedRoute>
              {/* Your existing app */}
            </ProtectedRoute>
          } />
          
          <Route path="/user-management" element={
            <ProtectedRoute requireAdmin>
              <UserManagement />
            </ProtectedRoute>
          } />
        </Routes>
      </Router>
    </AuthProvider>
  );
}
```

### Step 5: Test Login
1. Start backend: `dotnet run` (in MutualFundScanner.API)
2. Start frontend: `npm start` (in frontend)
3. Navigate to `http://localhost:3000/login`
4. Login with:
   - Username: `admin`
   - Password: `Admin@123`

## 📁 Files Created

### Backend
- ✅ `Domain/Entities/User.cs` - User entity
- ✅ `Domain/Enums/UserRole.cs` - Role enum
- ✅ `Application/DTOs/AuthDto.cs` - Authentication DTOs
- ✅ `Application/Services/IAuthService.cs` - Service interface
- ✅ `Application/Services/AuthService.cs` - Service implementation
- ✅ `API/Controllers/AuthController.cs` - Auth endpoints
- ✅ `API/Controllers/UserManagementController.cs` - User management endpoints
- ✅ `Infrastructure/Data/MutualFundContext.cs` - Updated with User entity
- ✅ `API/Program.cs` - Updated with JWT configuration
- ✅ `API/appsettings.json` - Updated with JWT settings

### Frontend
- ✅ `types/auth.ts` - TypeScript types
- ✅ `contexts/AuthContext.tsx` - Auth context provider
- ✅ `services/authApi.ts` - Auth API service
- ✅ `services/api.ts` - Updated with auth interceptors
- ✅ `components/ProtectedRoute.tsx` - Route protection
- ✅ `pages/Login.tsx` - Login page
- ✅ `pages/UserManagement.tsx` - User management UI

## 🔑 Key Features

### Authentication
- ✅ JWT token-based authentication
- ✅ Secure password hashing with BCrypt
- ✅ Token expiration (24 hours default)
- ✅ Automatic token refresh on page reload
- ✅ 401 handling with auto-redirect to login

### Authorization
- ✅ Role-based access control (Admin, ServiceUser)
- ✅ Protected routes in frontend
- ✅ `[Authorize]` attributes in backend
- ✅ `[Authorize(Roles = "Admin")]` for admin-only endpoints

### User Management (Admin Only)
- ✅ Create users with username, email, password, role
- ✅ Update user details (email, role, notes)
- ✅ Delete users
- ✅ Activate/Deactivate users
- ✅ Reset user passwords
- ✅ View all users with filtering

## 🔒 Security Features

- **Password Hashing**: BCrypt with salt
- **JWT Tokens**: Signed with secret key
- **Token Validation**: Issuer, Audience, Expiration
- **HTTPS Ready**: Configure for production
- **CORS Protection**: Configured for specific origins
- **Self-Protection**: Users cannot delete/deactivate themselves

## 📝 Default Credentials

**Username**: `admin`  
**Password**: `Admin@123`  
**Role**: Admin

⚠️ **IMPORTANT**: Change the default password immediately after first login!

## 🎯 User Roles

### Admin
- Full system access
- User management
- All application features

### ServiceUser
- Application features only
- Cannot manage users
- Can change own password

## 🔧 Configuration

### JWT Settings (appsettings.json)
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGenerationMustBeAtLeast32CharactersLong!",
    "Issuer": "MutualFundScanner",
    "Audience": "MutualFundScannerUsers",
    "ExpirationHours": 24
  }
}
```

⚠️ **Change SecretKey in production!**

## 🧪 Testing

### Test Login API
```bash
curl -X POST http://localhost:5247/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}'
```

### Test Protected Endpoint
```bash
curl -X GET http://localhost:5247/api/auth/me \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Test User Creation
```bash
curl -X POST http://localhost:5247/api/usermanagement \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "user1",
    "email": "user1@example.com",
    "password": "Password123",
    "role": "ServiceUser"
  }'
```

## 📚 API Endpoints

### Public
- `POST /api/auth/login` - Login

### Authenticated
- `GET /api/auth/me` - Get current user
- `POST /api/auth/change-password` - Change password

### Admin Only
- `GET /api/usermanagement` - List users
- `POST /api/usermanagement` - Create user
- `PUT /api/usermanagement/{id}` - Update user
- `DELETE /api/usermanagement/{id}` - Delete user
- `POST /api/usermanagement/{id}/activate` - Activate
- `POST /api/usermanagement/{id}/deactivate` - Deactivate
- `POST /api/usermanagement/reset-password` - Reset password

## 🐛 Troubleshooting

**Issue**: Cannot login  
**Solution**: Verify user exists and is active in database

**Issue**: 401 on all requests  
**Solution**: Check token in localStorage and Authorization header

**Issue**: NuGet package errors  
**Solution**: Run `dotnet restore` in each project

**Issue**: Migration fails  
**Solution**: Check connection string and SQL Server is running

## 📖 Full Documentation

See `AUTHENTICATION_IMPLEMENTATION_GUIDE.md` for complete details.

## ✅ Checklist

- [ ] Install NuGet packages
- [ ] Run database migration
- [ ] Create admin user
- [ ] Update JWT secret key
- [ ] Update App.tsx with AuthProvider
- [ ] Add user menu to layout
- [ ] Test login functionality
- [ ] Create additional users
- [ ] Change default admin password
- [ ] Configure for production

## 🎉 You're Done!

Your Mutual Fund Scanner now has a complete authentication and authorization system with role-based access control!
