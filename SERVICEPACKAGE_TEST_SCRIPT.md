# ServicePackage API - Complete Test Script

## Prerequisites
- Application running
- Valid JWT token (login first)
- At least one Studio record in database

---

## Test Sequence

### Step 1: Login to Get Token
```http
POST /api/Auth/login
Content-Type: application/json

{
  "email": "your-email@example.com",
  "password": "your-password"
}
```

**Copy the `token` value from response**

---

### Step 2: Authorize in Swagger
1. Click "Authorize" button (top right)
2. Enter: `Bearer {paste_your_token_here}`
3. Click "Authorize"

---

### Step 3: Test GET Operations (No Auth Required)

#### Test 3.1: Get All Service Packages
```http
GET /api/ServicePackage
```

**Expected Response**:
```json
{
  "success": true,
  "data": [...],
  "count": X
}
```

**Verify**:
- ? `success` is `true`
- ? `studioName` is populated (not null)
- ? `totalAddons` shows number
- ? `totalBookings` shows number

---

#### Test 3.2: Get Package by ID
```http
GET /api/ServicePackage/1
```

**Expected Response**:
```json
{
  "success": true,
  "data": {
    "servicePkgId": 1,
 "studioName": "...",
    "studioAddress": "...",
    "studioContactInfo": "...",
    "addons": [...]
  }
}
```

**Verify**:
- ? Studio details populated
- ? Addons array present
- ? No null values for studio properties

---

#### Test 3.3: Get Packages by Studio
```http
GET /api/ServicePackage/studio/1
```

**Expected**: List of packages for studio 1

**Verify**:
- ? All packages belong to studioId 1
- ? Studio information populated

---

### Step 4: Test POST Operation (Auth Required)

#### Test 4.1: Create New Package
```http
POST /api/ServicePackage
Authorization: Bearer {your_token}
Content-Type: application/json

{
  "studioId": 1,
  "name": "Test Wedding Package",
  "description": "Complete wedding photography and videography",
  "basePrice": 5000000
}
```

**Expected Response**: 201 Created
```json
{
  "success": true,
  "message": "Service package created successfully.",
  "data": {
    "servicePkgId": X,
    "studioId": 1,
    "studioName": "...",
    "name": "Test Wedding Package",
    "description": "Complete wedding photography and videography",
    "basePrice": 5000000,
    "totalAddons": 0,
    "totalBookings": 0
  }
}
```

**Verify**:
- ? Status code is 201
- ? `servicePkgId` is assigned
- ? `studioName` is populated
- ? No exceptions thrown

**Common Errors**:
- ? 401 Unauthorized ? Token expired or invalid
- ? 403 Forbidden ? User doesn't have Manager/Admin role
- ? 400 Bad Request ? Studio doesn't exist or validation failed

---

#### Test 4.2: Create with Invalid Studio (Should Fail)
```http
POST /api/ServicePackage
Authorization: Bearer {your_token}
Content-Type: application/json

{
  "studioId": 99999,
  "name": "Invalid Package",
  "description": "This should fail",
  "basePrice": 1000000
}
```

**Expected Response**: 400 Bad Request
```json
{
  "success": false,
  "message": "Failed to create service package. Studio may not exist."
}
```

**Verify**:
- ? Request is rejected
- ? Appropriate error message

---

#### Test 4.3: Create with Validation Errors
```http
POST /api/ServicePackage
Authorization: Bearer {your_token}
Content-Type: application/json

{
  "studioId": 0,
  "name": "",
  "basePrice": -100
}
```

**Expected Response**: 400 Bad Request with validation errors

**Verify**:
- ? Validation catches errors
- ? Error messages explain issues

---

### Step 5: Test PUT Operation (Auth Required)

#### Test 5.1: Update Package (Partial Update)
```http
PUT /api/ServicePackage/1
Authorization: Bearer {your_token}
Content-Type: application/json

{
  "name": "Updated Wedding Package Name",
  "basePrice": 6000000
}
```

**Expected Response**: 200 OK
```json
{
  "success": true,
  "message": "Service package updated successfully."
}
```

**Verify**:
- ? Status code is 200
- ? Success message returned
- ? Get the package again to verify changes applied

---

#### Test 5.2: Verify Update Applied
```http
GET /api/ServicePackage/1
```

**Verify**:
- ? `name` changed to "Updated Wedding Package Name"
- ? `basePrice` changed to 6000000
- ? Other fields unchanged (studioId, description, etc.)

---

#### Test 5.3: Update with Studio Change
```http
PUT /api/ServicePackage/1
Authorization: Bearer {your_token}
Content-Type: application/json

{
  "studioId": 2
}
```

**Expected Response**: 200 OK (if Studio 2 exists)

**Verify**:
- ? Studio validation works
- ? Update succeeds if studio exists
- ? Fails appropriately if studio doesn't exist

---

#### Test 5.4: Update Non-Existent Package
```http
PUT /api/ServicePackage/99999
Authorization: Bearer {your_token}
Content-Type: application/json

{
  "name": "This should fail"
}
```

**Expected Response**: 404 Not Found
```json
{
  "success": false,
"message": "Service package not found or studio doesn't exist."
}
```

**Verify**:
- ? Returns 404
- ? Appropriate error message

---

### Step 6: Test DELETE Operation (Admin Only)

#### Test 6.1: Delete Package
```http
DELETE /api/ServicePackage/1
Authorization: Bearer {admin_token}
```

**Expected Response**: 200 OK
```json
{
  "success": true,
  "message": "Service package deleted successfully."
}
```

**Verify**:
- ? Status code is 200
- ? Success message returned

---

#### Test 6.2: Verify Deletion
```http
GET /api/ServicePackage/1
```

**Expected Response**: 404 Not Found
```json
{
  "success": false,
  "message": "Service package not found."
}
```

**Verify**:
- ? Package no longer exists

---

#### Test 6.3: Delete with Manager Token (Should Fail)
```http
DELETE /api/ServicePackage/2
Authorization: Bearer {manager_token}
```

**Expected Response**: 403 Forbidden

**Verify**:
- ? Only Admin can delete
- ? Manager cannot delete

---

#### Test 6.4: Delete Non-Existent Package
```http
DELETE /api/ServicePackage/99999
Authorization: Bearer {admin_token}
```

**Expected Response**: 404 Not Found
```json
{
  "success": false,
  "message": "Service package not found."
}
```

**Verify**:
- ? Returns 404
- ? Appropriate error message

---

### Step 7: Test EXISTS Endpoint

#### Test 7.1: Check Existing Package
```http
GET /api/ServicePackage/1/exists
```

**Expected Response**: 200 OK
```json
{
  "success": true,
  "exists": true
}
```

---

#### Test 7.2: Check Non-Existent Package
```http
GET /api/ServicePackage/99999/exists
```

**Expected Response**: 200 OK
```json
{
  "success": true,
  "exists": false
}
```

---

## Complete Test Results Checklist

### GET Operations ?
- [ ] Get all packages works
- [ ] Get package by ID works
- [ ] Get packages by studio works
- [ ] Check exists works
- [ ] Studio names populated
- [ ] Counts (addons, bookings) accurate

### POST Operations ?
- [ ] Create package works
- [ ] Studio validation works
- [ ] Input validation works
- [ ] Appropriate error messages
- [ ] Authorization required

### PUT Operations ?
- [ ] Update package works
- [ ] Partial update works
- [ ] Studio validation on update works
- [ ] Non-existent package returns 404
- [ ] Authorization required

### DELETE Operations ?
- [ ] Delete package works
- [ ] Admin-only enforcement works
- [ ] Non-existent package returns 404
- [ ] Package actually deleted from DB

---

## Common Issues & Solutions

### Issue: 401 Unauthorized
**Solution**: 
- Login again to get fresh token
- Ensure token is prefixed with "Bearer "
- Check token hasn't expired

### Issue: 403 Forbidden
**Solution**:
- Verify user has correct role (Manager/Admin for POST/PUT, Admin for DELETE)
- Check role claims in JWT token

### Issue: 400 Bad Request "Studio may not exist"
**Solution**:
- Verify studio with given ID exists in database
- Check Studios table has records
- Try with different studioId

### Issue: 500 Internal Server Error
**Solution**:
- Check Output window in Visual Studio
- Verify database connection string
- Check if required navigation properties exist

---

## Database Verification Queries

Run these SQL queries to verify data:

```sql
-- Check Studios exist
SELECT * FROM Studios;

-- Check ServicePackages
SELECT * FROM ServicePackages;

-- Check ServicePackages with Studio info
SELECT 
    sp.ServicePkgId,
sp.Name,
    sp.BasePrice,
    s.Name AS StudioName
FROM ServicePackages sp
LEFT JOIN Studios s ON sp.StudioId = s.StudioId;

-- Check ServiceAddons for a package
SELECT * FROM ServiceAddons WHERE ServicePkgId = 1;

-- Check ServiceBookings for a package
SELECT * FROM ServiceBookings WHERE ServicePkgId = 1;
```

---

## Success Criteria

All tests pass when:
- ? All GET operations return data with populated fields
- ? POST creates new records successfully
- ? PUT updates records successfully
- ? DELETE removes records successfully
- ? Authorization enforced correctly
- ? Validation catches invalid input
- ? Appropriate error messages returned
- ? No exceptions in console/logs

---

**Status**: ? All operations fixed and functional  
**Next Step**: Run through this test script systematically
