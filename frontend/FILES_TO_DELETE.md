# Frontend Files to Delete - Complete List

## Summary
**Total:** 7 files + 2 directories to delete

## Detailed Deletion List

### 1. Page Components (2 files)

#### Delete: `src/pages/FileUploadPage.tsx`
**Reason:** Legacy upload page, replaced by Smart Upload V2
**Used in:** Removed from routes in App.tsx

#### Delete: `src/pages/CurrentHoldingsPage.tsx`
**Reason:** Part of removed Holdings submenu
**Used in:** Removed from routes in App.tsx

---

### 2. Component Directory: FileUpload (DELETE ENTIRE DIRECTORY)

#### Delete: `src/components/FileUpload/`
**Contains 3 files:**
- `EnhancedFileUpload.tsx` (13,495 bytes)
- `FileUploadArea.tsx` (3,035 bytes)
- `index.ts` (71 bytes)

**Reason:** Legacy file upload components, replaced by UploadV2
**Used in:** FileUploadPage.tsx (which is also being deleted)

---

### 3. Component Directory: Holdings (DELETE ENTIRE DIRECTORY)

#### Delete: `src/components/Holdings/`
**Contains 2 files:**
- `HoldingsComparison.tsx` (5,024 bytes)
- `PortfolioSummary.tsx` (6,571 bytes)

**Reason:** Part of removed Holdings submenu
**Used in:** Routes in App.tsx (which have been removed)

---

## Files Already Updated (DO NOT DELETE)

### ✅ `src/pages/index.ts`
**Status:** Updated
**Changes:**
- Removed `FileUploadPage` export
- Changed default export from `FileUploadPage` to `AnalysePage`

### ✅ `src/App.tsx`
**Status:** Updated
**Changes:**
- Removed imports for deleted components
- Removed routes for deleted pages
- Changed default route to `/analyze`

### ✅ `src/components/Layout/AppLayout.tsx`
**Status:** Updated
**Changes:**
- Removed "Upload File" menu item
- Removed "Holdings" submenu
- Changed default selected key to '2' (Analyse)

---

## Deletion Commands

### Windows (PowerShell):
```powershell
# Navigate to frontend directory
cd d:\mf-scanner\frontend\src

# Delete page files
Remove-Item pages\FileUploadPage.tsx
Remove-Item pages\CurrentHoldingsPage.tsx

# Delete component directories
Remove-Item -Recurse components\FileUpload
Remove-Item -Recurse components\Holdings
```

### Linux/Mac (Bash):
```bash
# Navigate to frontend directory
cd /d/mf-scanner/frontend/src

# Delete page files
rm pages/FileUploadPage.tsx
rm pages/CurrentHoldingsPage.tsx

# Delete component directories
rm -rf components/FileUpload
rm -rf components/Holdings
```

---

## Verification Checklist

After deletion, verify:

- [ ] No import errors in console
- [ ] Application builds successfully
- [ ] All remaining routes work correctly
- [ ] No references to deleted components in code
- [ ] Navigate to `/analyze` works (default page)
- [ ] Smart Upload V2 (`/upload-v2`) still works
- [ ] Portfolio Holdings (`/portfolio-holdings`) still works

---

## What Remains (Active Components)

### Active Pages:
✅ `AnalysePage.tsx` - Main analytics page (new default)
✅ `MutualFundListPage.tsx` - Mutual fund list
✅ `PortfolioHoldingsPage.tsx` - Portfolio holdings view
✅ `ConcreteMutualFunds.tsx` - Concrete mutual funds management

### Active Upload:
✅ `components/Upload/UploadV2.tsx` - Smart Upload V2 (only upload option)

### Active Components:
✅ `components/Analytics/` - All analytics components
✅ `components/Layout/` - Layout components
✅ `components/MutualFund/` - Mutual fund components
✅ `components/FileList/` - File list components

---

## Impact Analysis

### Before Cleanup:
- 2 upload options (confusing)
- 4 holdings-related pages (redundant)
- 9 total menu items
- Default landing: Upload File page

### After Cleanup:
- 1 upload option (Smart Upload V2)
- 1 holdings page (Portfolio Holdings)
- 6 total menu items
- Default landing: Analyse page

### Space Saved:
- **~28 KB** of code removed
- **2 directories** removed
- **7 files** removed

---

## Rollback Plan (If Needed)

If you need to restore deleted files:

1. Use Git to restore:
```bash
git checkout HEAD -- src/pages/FileUploadPage.tsx
git checkout HEAD -- src/pages/CurrentHoldingsPage.tsx
git checkout HEAD -- src/components/FileUpload
git checkout HEAD -- src/components/Holdings
```

2. Revert changes in:
- `src/App.tsx`
- `src/components/Layout/AppLayout.tsx`
- `src/pages/index.ts`

---

## Summary

**Action Required:** Delete 7 files and 2 directories listed above

**Files to Delete:**
1. `src/pages/FileUploadPage.tsx`
2. `src/pages/CurrentHoldingsPage.tsx`
3. `src/components/FileUpload/EnhancedFileUpload.tsx`
4. `src/components/FileUpload/FileUploadArea.tsx`
5. `src/components/FileUpload/index.ts`
6. `src/components/Holdings/HoldingsComparison.tsx`
7. `src/components/Holdings/PortfolioSummary.tsx`

**Directories to Delete:**
1. `src/components/FileUpload/`
2. `src/components/Holdings/`

**Result:** Clean, streamlined frontend with single upload option and focused analytics!
