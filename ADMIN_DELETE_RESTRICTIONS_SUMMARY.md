# Admin-Only Delete Functionality Implementation

## Overview
Implemented role-based access control to restrict delete operations to Admin users only, both on the backend API and frontend UI.

## Backend Changes

### Controllers Updated with Admin-Only Delete Authorization

#### 1. **MutualFundController.cs**
- **File**: `backend/MutualFundScanner.API/Controllers/MutualFundController.cs`
- **Changes**:
  - Added `using Microsoft.AspNetCore.Authorization;`
  - Added `[Authorize(Roles = "Admin")]` to:
    - `DeleteMutualFund(int id)` - Single mutual fund deletion
    - `HardDeleteMutualFunds([FromBody] DeleteMutualFundsRequestDto request)` - Bulk deletion

#### 2. **SnapshotManagementController.cs**
- **File**: `backend/MutualFundScanner.API/Controllers/SnapshotManagementController.cs`
- **Changes**:
  - Added `using Microsoft.AspNetCore.Authorization;`
  - Added `[Authorize(Roles = "Admin")]` to:
    - `DeleteSnapshot([FromBody] DeleteSnapshotRequestDto request)` - Single snapshot deletion
    - `BulkDeleteSnapshots([FromBody] BulkDeleteSnapshotRequestDto request)` - Bulk snapshot deletion

#### 3. **ConcreteMutualFundController.cs** (Already Admin-Only)
- **File**: `backend/MutualFundScanner.API/Controllers/ConcreteMutualFundController.cs`
- **Status**: Entire controller already restricted to Admin with `[Authorize(Roles = "Admin")]` at controller level
- Includes delete endpoint: `Delete(int id)`

#### 4. **UserManagementController.cs** (Already Admin-Only)
- **File**: `backend/MutualFundScanner.API/Controllers/UserManagementController.cs`
- **Status**: Entire controller already restricted to Admin
- Includes delete endpoint: `DeleteUser(int userId)`

#### 5. **BackgroundProcessingController.cs** (Already Admin-Only)
- **File**: `backend/MutualFundScanner.API/Controllers/BackgroundProcessingController.cs`
- **Status**: Entire controller already restricted to Admin
- Includes cancel endpoint (similar to delete): `CancelJob(string jobId)`

#### 6. **UploadV2Controller.cs** (Already Admin-Only)
- **File**: `backend/MutualFundScanner.API/Controllers/UploadV2Controller.cs`
- **Status**: Entire controller already restricted to Admin

## Frontend Changes

### Pages Updated to Hide Delete Buttons for Non-Admin Users

#### 1. **SnapshotManagementPage.tsx**
- **File**: `frontend/src/pages/SnapshotManagementPage.tsx`
- **Changes**:
  - Added `import { useAuth } from '../contexts/AuthContext';`
  - Added `const { isAdmin } = useAuth();` hook
  - **Single Snapshot Delete Button**: Wrapped in `{isAdmin && (...)}`
    - Located in the "Snapshot Details" card extra section
    - Only visible to Admin users
  - **Bulk Delete Tab**: Conditionally rendered using spread operator
    - Entire "Bulk Delete" tab hidden for non-admin users
    - Uses: `...(isAdmin ? [{...}] : [])`

#### 2. **AppLayout.tsx** (Previously Updated)
- **File**: `frontend/src/components/Layout/AppLayout.tsx`
- **Admin-Only Menu Items**:
  - Concrete Mutual Funds
  - Smart Upload V2
  - Background Processing
  - User Management

#### 3. **App.tsx** (Previously Updated)
- **File**: `frontend/src/App.tsx`
- **Admin-Only Routes**:
  - `/upload-v2` - Protected with `<ProtectedRoute requireAdmin>`
  - `/concrete-funds` - Protected with `<ProtectedRoute requireAdmin>`
  - `/background-processing` - Protected with `<ProtectedRoute requireAdmin>`
  - `/user-management` - Protected with `<ProtectedRoute requireAdmin>`

## Security Model

### Backend Authorization
- **Role-Based**: Uses `[Authorize(Roles = "Admin")]` attribute
- **JWT Claims**: Role claim included in token via `ClaimTypes.Role`
- **Response**: Returns **403 Forbidden** for non-admin users attempting delete operations

### Frontend Protection
- **UI Hiding**: Delete buttons/tabs hidden using `isAdmin` check
- **Route Protection**: Admin-only pages wrapped in `<ProtectedRoute requireAdmin>`
- **Context**: Uses `useAuth()` hook from `AuthContext` to check user role

## User Experience

### Admin Users
- ✅ See all delete buttons and bulk delete functionality
- ✅ Can access all admin-only menu items
- ✅ Full CRUD operations on all entities

### ServiceUser Role
- ❌ Delete buttons hidden in Snapshot Management
- ❌ Bulk Delete tab not visible
- ❌ Cannot access Concrete Funds, Upload V2, Background Processing
- ✅ Can view and analyze data
- ✅ Can access Analytics, Security Analytics, Mutual Fund List, Snapshot Management (view only)

## API Endpoints with Delete Restrictions

| Endpoint | Method | Controller | Authorization |
|----------|--------|------------|---------------|
| `/api/MutualFund/{id}` | DELETE | MutualFundController | Admin Only |
| `/api/MutualFund/hard-delete` | DELETE | MutualFundController | Admin Only |
| `/api/SnapshotManagement/snapshot` | DELETE | SnapshotManagementController | Admin Only |
| `/api/SnapshotManagement/snapshots/bulk` | DELETE | SnapshotManagementController | Admin Only |
| `/api/ConcreteMutualFund/{id}` | DELETE | ConcreteMutualFundController | Admin Only (Entire Controller) |
| `/api/usermanagement/{userId}` | DELETE | UserManagementController | Admin Only (Entire Controller) |

## Testing Checklist

### As Admin User
- [x] Login with Admin credentials
- [x] Verify delete button visible in Snapshot Management
- [x] Verify Bulk Delete tab visible
- [x] Verify delete operations succeed (200 OK)
- [x] Verify all admin menu items visible

### As ServiceUser
- [x] Login with ServiceUser credentials
- [x] Verify delete button hidden in Snapshot Management
- [x] Verify Bulk Delete tab hidden
- [x] Verify direct API calls to delete endpoints return 403
- [x] Verify admin menu items hidden
- [x] Verify redirect when accessing admin routes directly

## Implementation Notes

1. **Consistent Pattern**: All delete operations follow the same authorization pattern
2. **Defense in Depth**: Protection at both UI (hiding) and API (authorization) levels
3. **User Feedback**: Non-admin users simply don't see delete options (better UX than showing disabled buttons)
4. **Maintainability**: Uses centralized `useAuth()` hook and `[Authorize]` attributes

## Files Modified

### Backend
- ✅ `backend/MutualFundScanner.API/Controllers/MutualFundController.cs`
- ✅ `backend/MutualFundScanner.API/Controllers/SnapshotManagementController.cs`
- ✅ `backend/MutualFundScanner.API/Controllers/ConcreteMutualFundController.cs` (already protected)
- ✅ `backend/MutualFundScanner.API/Controllers/UserManagementController.cs` (already protected)
- ✅ `backend/MutualFundScanner.API/Controllers/BackgroundProcessingController.cs` (already protected)
- ✅ `backend/MutualFundScanner.API/Controllers/UploadV2Controller.cs` (already protected)

### Frontend
- ✅ `frontend/src/pages/SnapshotManagementPage.tsx`
- ✅ `frontend/src/components/Layout/AppLayout.tsx` (already updated)
- ✅ `frontend/src/App.tsx` (already updated)

## Summary

All delete functionality across the Mutual Fund Scanner application is now restricted to Admin users only. This includes:
- Single and bulk snapshot deletions
- Mutual fund deletions
- Concrete mutual fund deletions
- User deletions
- Background job cancellations

The implementation provides comprehensive security through both backend authorization and frontend UI controls, ensuring ServiceUser accounts have read-only access while Admins retain full control.
