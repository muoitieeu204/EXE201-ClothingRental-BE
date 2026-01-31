# ServicePackage CRUD API Documentation

## Overview
This document provides comprehensive information about the ServicePackage CRUD API implementation for the EXE201 Clothing Rental system.

## File Structure

### DTOs (Data Transfer Objects)
Location: `EXE201.service\DTOs\ServicePackageDTOs\`

1. **CreateServicePackageDto.cs** - Used for creating new service packages
2. **UpdateServicePackageDto.cs** - Used for updating existing service packages
3. **ServicePackageResponseDto.cs** - Standard response for list operations
4. **ServicePackageDetailDto.cs** - Detailed response including related data

### Service Layer
- **Interface**: `EXE201.service\Interface\IServicePackageService.cs`
- **Implementation**: `EXE201.service\Implementation\ServicePackageService.cs`

### Controller
- **Location**: `EXE201\Controllers\ServicePackageController.cs`
- **Route**: `api/ServicePackage`

### Mappings
- **Location**: `EXE201.service\Mapper\MappingProfile.cs`
- AutoMapper configurations for ServicePackage entity mappings

---

## API Endpoints

### 1. Get All Service Packages
**GET** `/api/ServicePackage`

**Description**: Retrieve all service packages

**Authorization**: None (Public)

**Response Example**:
```json
{
  "success": true,
  "data": [
    {
      "servicePkgId": 1,
      "studioId": 1,
   "studioName": "Studio A",
    "name": "Wedding Photography Package",
      "description": "Complete wedding photography service",
      "basePrice": 5000000,
      "totalAddons": 3,
  "totalBookings": 15
    }
  ],
  "count": 1
}
```

---

### 2. Get Service Package by ID
**GET** `/api/ServicePackage/{id}`

**Description**: Retrieve detailed information about a specific service package

**Authorization**: None (Public)

**Parameters**:
- `id` (path, required): Service package ID

**Response Example**:
```json
{
  "success": true,
  "data": {
    "servicePkgId": 1,
    "studioId": 1,
    "studioName": "Studio A",
"studioAddress": "123 Main St",
    "studioContactInfo": "contact@studioa.com",
    "name": "Wedding Photography Package",
    "description": "Complete wedding photography service",
    "basePrice": 5000000,
    "addons": [
      {
        "addonId": 1,
        "name": "Extra Hour",
        "price": 500000
      }
    ],
    "totalBookings": 15
  }
}
```

**Error Responses**:
- `400 Bad Request`: Invalid ID
- `404 Not Found`: Service package not found

---

### 3. Get Service Packages by Studio ID
**GET** `/api/ServicePackage/studio/{studioId}`

**Description**: Retrieve all service packages for a specific studio

**Authorization**: None (Public)

**Parameters**:
- `studioId` (path, required): Studio ID

**Response Example**:
```json
{
  "success": true,
  "data": [
    {
    "servicePkgId": 1,
      "studioId": 1,
 "studioName": "Studio A",
 "name": "Wedding Photography Package",
    "description": "Complete wedding photography service",
      "basePrice": 5000000,
      "totalAddons": 3,
  "totalBookings": 15
    }
  ],
  "count": 1
}
```

**Error Responses**:
- `400 Bad Request`: Invalid studio ID

---

### 4. Create Service Package
**POST** `/api/ServicePackage`

**Description**: Create a new service package

**Authorization**: Required - Roles: `Admin`, `Manager`

**Request Body**:
```json
{
  "studioId": 1,
  "name": "Wedding Photography Package",
  "description": "Complete wedding photography service",
  "basePrice": 5000000
}
```

**Validation Rules**:
- `studioId`: Required, must be > 0
- `name`: Required, 1-255 characters
- `description`: Optional, max 1000 characters
- `basePrice`: Required, must be non-negative

**Response Example**:
```json
{
  "success": true,
  "message": "Service package created successfully.",
  "data": {
    "servicePkgId": 1,
    "studioId": 1,
    "studioName": "Studio A",
    "name": "Wedding Photography Package",
    "description": "Complete wedding photography service",
    "basePrice": 5000000,
  "totalAddons": 0,
    "totalBookings": 0
  }
}
```

**Error Responses**:
- `400 Bad Request`: Invalid data or studio doesn't exist
- `401 Unauthorized`: Not authenticated
- `403 Forbidden`: Insufficient permissions

---

### 5. Update Service Package
**PUT** `/api/ServicePackage/{id}`

**Description**: Update an existing service package

**Authorization**: Required - Roles: `Admin`, `Manager`

**Parameters**:
- `id` (path, required): Service package ID

**Request Body** (all fields optional):
```json
{
  "studioId": 1,
  "name": "Updated Wedding Package",
  "description": "Updated description",
  "basePrice": 6000000
}
```

**Validation Rules**:
- `studioId`: Optional, must be > 0 if provided
- `name`: Optional, 1-255 characters if provided
- `description`: Optional, max 1000 characters if provided
- `basePrice`: Optional, must be non-negative if provided

**Response Example**:
```json
{
  "success": true,
  "message": "Service package updated successfully."
}
```

**Error Responses**:
- `400 Bad Request`: Invalid ID or data
- `404 Not Found`: Service package or studio not found
- `401 Unauthorized`: Not authenticated
- `403 Forbidden`: Insufficient permissions

---

### 6. Delete Service Package
**DELETE** `/api/ServicePackage/{id}`

**Description**: Delete a service package

**Authorization**: Required - Role: `Admin`

**Parameters**:
- `id` (path, required): Service package ID

**Response Example**:
```json
{
  "success": true,
  "message": "Service package deleted successfully."
}
```

**Error Responses**:
- `400 Bad Request`: Invalid ID
- `404 Not Found`: Service package not found
- `401 Unauthorized`: Not authenticated
- `403 Forbidden`: Insufficient permissions

---

### 7. Check Service Package Exists
**GET** `/api/ServicePackage/{id}/exists`

**Description**: Check if a service package exists

**Authorization**: None (Public)

**Parameters**:
- `id` (path, required): Service package ID

**Response Example**:
```json
{
  "success": true,
  "exists": true
}
```

**Error Responses**:
- `400 Bad Request`: Invalid ID

---

## DTOs Details

### CreateServicePackageDto
```csharp
public class CreateServicePackageDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int StudioId { get; set; }

    [Required]
  [StringLength(255, MinimumLength = 1)]
    public string Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

[Required]
    [Range(0, double.MaxValue)]
    public decimal BasePrice { get; set; }
}
```

### UpdateServicePackageDto
```csharp
public class UpdateServicePackageDto
{
    [Range(1, int.MaxValue)]
    public int? StudioId { get; set; }

    [StringLength(255, MinimumLength = 1)]
    public string? Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
 public decimal? BasePrice { get; set; }
}
```

### ServicePackageResponseDto
```csharp
public class ServicePackageResponseDto
{
    public int ServicePkgId { get; set; }
    public int StudioId { get; set; }
  public string? StudioName { get; set; }
    public string Name { get; set; }
  public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public int TotalAddons { get; set; }
    public int TotalBookings { get; set; }
}
```

### ServicePackageDetailDto
```csharp
public class ServicePackageDetailDto
{
    public int ServicePkgId { get; set; }
    public int StudioId { get; set; }
    public string? StudioName { get; set; }
  public string? StudioAddress { get; set; }
    public string? StudioContactInfo { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public List<ServiceAddonInfo>? Addons { get; set; }
    public int TotalBookings { get; set; }
}

public class ServiceAddonInfo
{
  public int AddonId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

---

## Service Layer

### IServicePackageService Interface
```csharp
public interface IServicePackageService
{
    Task<IEnumerable<ServicePackageResponseDto>> GetAllAsync();
    Task<ServicePackageDetailDto?> GetByIdAsync(int id);
    Task<IEnumerable<ServicePackageResponseDto>> GetPackagesByStudioIdAsync(int studioId);
    Task<ServicePackageResponseDto?> CreateAsync(CreateServicePackageDto dto);
 Task<bool> UpdateAsync(int id, UpdateServicePackageDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
```

### Key Features
1. **Studio Validation**: Verifies studio exists before creating/updating packages
2. **AutoMapper Integration**: Automatic entity-DTO mapping
3. **Unit of Work Pattern**: Consistent data access through IUnitOfWork
4. **Async Operations**: All database operations are asynchronous

---

## Testing Guide

### Testing with Swagger

1. **Start the application**
2. **Navigate to Swagger UI**: `https://localhost:{port}/swagger`
3. **Authentication** (for protected endpoints):
   - Click "Authorize" button
   - Enter: `Bearer {your_jwt_token}`
   - Click "Authorize"

### Sample Test Cases

#### Test Case 1: Get All Service Packages
```http
GET /api/ServicePackage
```
Expected: List of all service packages

#### Test Case 2: Create Service Package (Requires Auth)
```http
POST /api/ServicePackage
Authorization: Bearer {token}
Content-Type: application/json

{
  "studioId": 1,
  "name": "Premium Wedding Package",
  "description": "Full day wedding photography with extras",
  "basePrice": 10000000
}
```
Expected: 201 Created with package details

#### Test Case 3: Update Service Package (Requires Auth)
```http
PUT /api/ServicePackage/1
Authorization: Bearer {token}
Content-Type: application/json

{
  "basePrice": 12000000,
  "description": "Updated description with new features"
}
```
Expected: 200 OK with success message

#### Test Case 4: Get Packages by Studio
```http
GET /api/ServicePackage/studio/1
```
Expected: List of packages for studio 1

#### Test Case 5: Delete Service Package (Admin Only)
```http
DELETE /api/ServicePackage/1
Authorization: Bearer {admin_token}
```
Expected: 200 OK with success message

---

## Error Handling

All endpoints follow a consistent error response format:

```json
{
  "success": false,
  "message": "Error description"
}
```

### Common HTTP Status Codes
- `200 OK`: Successful operation
- `201 Created`: Resource created successfully
- `400 Bad Request`: Invalid input data
- `401 Unauthorized`: Authentication required
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

---

## Dependencies

### Required Packages
- `AutoMapper` - Object-object mapping
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT authentication
- `Microsoft.EntityFrameworkCore` - Database operations

### Related Entities
- `ServicePackage` - Main entity
- `Studio` - Related studio information
- `ServiceAddon` - Package add-ons
- `ServiceBooking` - Package bookings

---

## Configuration

### Registration in Program.cs
```csharp
// Service Layer
builder.Services.AddScoped<IServicePackageService, ServicePackageService>();
```

### AutoMapper Profile
Mappings are registered in `MappingProfile.cs`:
- `ServicePackage` ? `ServicePackageResponseDto`
- `ServicePackage` ? `ServicePackageDetailDto`
- `CreateServicePackageDto` ? `ServicePackage`
- `UpdateServicePackageDto` ? `ServicePackage` (partial update)
- `ServiceAddon` ? `ServiceAddonInfo`

---

## Best Practices

1. **Always validate studio exists** before creating/updating packages
2. **Use appropriate authorization** for sensitive operations
3. **Return detailed error messages** for better debugging
4. **Leverage AutoMapper** for consistent DTO-Entity mapping
5. **Use Unit of Work** for transactional consistency
6. **Include related data** in detail endpoints for better UX

---

## Future Enhancements

1. **Pagination**: Add pagination for GetAll endpoint
2. **Filtering**: Add filters by price range, studio, etc.
3. **Sorting**: Add sorting options for list results
4. **Search**: Add full-text search functionality
5. **Caching**: Implement caching for frequently accessed packages
6. **Soft Delete**: Implement soft delete instead of hard delete
7. **Audit Trail**: Track changes to packages
8. **Bulk Operations**: Add bulk create/update/delete endpoints

---

## Related Documentation
- [OUTFITSIZE_CRUD_DOCUMENTATION.md](OUTFITSIZE_CRUD_DOCUMENTATION.md)
- Entity Framework Models: `EXE201.Repository\Models\ServicePackage.cs`
- Repository Pattern: `EXE201.Repository\Interfaces\IServicePackageRepository.cs`

---

## Support
For issues or questions, please contact the development team or refer to the project repository.

**Last Updated**: $(date)
**Version**: 1.0.0
