# Outfit CRUD Implementation

## ?? Overview
Complete CRUD implementation for the Outfit entity with advanced features including search, filtering, and detailed responses with navigation properties.

---

## ?? File Structure

### DTOs Created (4 files)
```
EXE201.service\DTOs\OutfitDTOs\
??? CreateOutfitDto.cs           (For POST requests)
??? UpdateOutfitDto.cs(For PUT requests)
??? OutfitResponseDto.cs  (For basic GET responses)
??? OutfitDetailDto.cs (For detailed GET with relations)
```

### Service Layer (2 files)
```
EXE201.service\Interface\
??? IOutfitService.cs       (Service interface with 10 methods)

EXE201.service\Implementation\
??? OutfitService.cs           (Service implementation)
```

### Controller (1 file)
```
EXE201\Controllers\
??? OutfitController.cs   (API endpoints - 9 endpoints)
```

---

## ?? DTO Specifications

### CreateOutfitDto
Used for creating new outfits.
```json
{
  "categoryId": 1,  // Required, must be > 0
  "name": "Summer Dress",          // Required, max 200 chars
  "type": "Dress",      // Optional, max 100 chars
  "gender": "Female",      // Optional, max 50 chars
  "region": "Western",     // Optional, max 100 chars
  "isLimited": false,       // Optional, boolean
  "status": "Available",        // Optional, max 50 chars
  "baseRentalPrice": 150.00        // Required, must be >= 0
}
```

**Validation Rules:**
- ? CategoryId: Required, must exist in database
- ? Name: Required, max 200 characters
- ? BaseRentalPrice: Required, non-negative
- ? Type, Gender, Region, Status: Optional with length limits

### UpdateOutfitDto
Used for updating existing outfits. **All fields are optional**.
```json
{
  "categoryId": 2,
  "name": "Updated Summer Dress",
  "type": "Formal Dress",
  "gender": "Unisex",
  "region": "Asian",
  "isLimited": true,
"status": "Low Stock",
  "baseRentalPrice": 175.00
}
```

### OutfitResponseDto
Basic outfit information with aggregated data.
```json
{
  "outfitId": 1,
  "categoryId": 1,
  "name": "Summer Dress",
  "type": "Dress",
  "gender": "Female",
  "region": "Western",
  "isLimited": false,
  "status": "Available",
  "baseRentalPrice": 150.00,
  "createdAt": "2024-01-15T10:30:00Z",
  
  "categoryName": "Dresses",
  "totalImages": 5,
  "totalSizes": 4,
  "availableSizes": 3,
  "primaryImageUrl": "https://example.com/image1.jpg",
  "averageRating": 4.5,
  "totalReviews": 23
}
```

### OutfitDetailDto
Detailed outfit with complete related data.
```json
{
  "outfitId": 1,
  "categoryId": 1,
  "name": "Summer Dress",
  "type": "Dress",
  "gender": "Female",
  "region": "Western",
  "isLimited": false,
  "status": "Available",
  "baseRentalPrice": 150.00,
  "createdAt": "2024-01-15T10:30:00Z",
  
  "categoryName": "Dresses",
  
  "images": [
    {
  "imageId": 1,
      "imageUrl": "https://example.com/image1.jpg",
    "imageType": "Primary",
      "sortOrder": 1
    }
  ],
  
  "sizes": [
    {
      "sizeId": 1,
      "sizeLabel": "M",
      "stockQuantity": 10,
      "status": "Available"
    }
  ],
  
  "averageRating": 4.5,
  "totalReviews": 23,
  
  "attributes": {
  "detailId": 1,
    "material": "Cotton",
 "silhouette": "A-Line",
    "formalityLevel": "Casual",
    "occasion": "Daily Wear",
    "colorPrimary": "Blue",
    "seasonSuitability": "Summer",
    "storyTitle": "Elegant Summer Collection",
    "storyContent": "Perfect for sunny days...",
    "culturalOrigin": "Western"
  }
}
```

---

## ?? API Endpoints

### 1. Get All Outfits
**GET** `/api/outfit`
- **Auth:** Not required (AllowAnonymous)
- **Response:** Array of `OutfitResponseDto`
```json
{
  "success": true,
  "data": [...],
  "count": 50
}
```

### 2. Get Available Outfits Only
**GET** `/api/outfit/available`
- **Auth:** Not required (AllowAnonymous)
- **Description:** Returns only outfits with status = "Available"
- **Response:** Array of `OutfitResponseDto`

### 3. Get Outfits by Category
**GET** `/api/outfit/category/{categoryId}`
- **Auth:** Not required (AllowAnonymous)
- **Parameters:** `categoryId` (int) - Category ID
- **Response:** Array of `OutfitResponseDto`

### 4. Get Outfit by ID (Basic)
**GET** `/api/outfit/{id}`
- **Auth:** Not required (AllowAnonymous)
- **Response:** Single `OutfitResponseDto`
```json
{
  "success": true,
  "data": { ... }
}
```

### 5. Get Outfit Details (Full)
**GET** `/api/outfit/{id}/detail`
- **Auth:** Not required (AllowAnonymous)
- **Description:** Returns complete outfit with images, sizes, reviews, and attributes
- **Response:** Single `OutfitDetailDto`

### 6. Search Outfits
**GET** `/api/outfit/search?q={searchTerm}`
- **Auth:** Not required (AllowAnonymous)
- **Parameters:** `q` (string) - Search term
- **Search Fields:** Name, Type, Gender, Region
- **Response:** Array of `OutfitResponseDto`

**Example:**
```
GET /api/outfit/search?q=dress
GET /api/outfit/search?q=summer
GET /api/outfit/search?q=female
```

### 7. Create Outfit
**POST** `/api/outfit`
- **Auth:** Required (JWT Bearer token)
- **Body:** `CreateOutfitDto`
- **Response:**
```json
{
  "success": true,
  "message": "Outfit created successfully.",
  "outfitId": 123
}
```

**Error Cases:**
- 400: Invalid data or category doesn't exist
- 401: Unauthorized
- 500: Server error

### 8. Update Outfit
**PUT** `/api/outfit/{id}`
- **Auth:** Required (JWT Bearer token)
- **Body:** `UpdateOutfitDto` (partial update supported)
- **Response:**
```json
{
  "success": true,
  "message": "Outfit updated successfully."
}
```

**Error Cases:**
- 400: Invalid ID or category doesn't exist
- 404: Outfit not found
- 401: Unauthorized
- 500: Server error

### 9. Delete Outfit
**DELETE** `/api/outfit/{id}`
- **Auth:** Required (JWT Bearer token)
- **Response:**
```json
{
  "success": true,
  "message": "Outfit deleted successfully."
}
```

**?? Warning:** Deleting an outfit will cascade delete:
- All outfit images
- All outfit sizes
- All outfit attributes
- All wishlists containing this outfit
- All reviews for this outfit

---

## ?? Authentication

### Public Endpoints (No Auth Required):
- ? GET `/api/outfit` - All outfits
- ? GET `/api/outfit/available` - Available outfits
- ? GET `/api/outfit/category/{categoryId}` - By category
- ? GET `/api/outfit/{id}` - By ID
- ? GET `/api/outfit/{id}/detail` - Detailed view
- ? GET `/api/outfit/search` - Search

### Protected Endpoints (JWT Required):
- ?? POST `/api/outfit` - Create
- ?? PUT `/api/outfit/{id}` - Update
- ?? DELETE `/api/outfit/{id}` - Delete

**Bearer Token Format:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## ??? AutoMapper Configuration

### Complex Mappings in `MappingProfile.cs`:

#### OutfitResponseDto Mapping
```csharp
CreateMap<Outfit, OutfitResponseDto>()
    // Category name from navigation property
    .ForMember(dest => dest.CategoryName, 
   opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null))
    
    // Count total images
    .ForMember(dest => dest.TotalImages, 
  opt => opt.MapFrom(src => src.OutfitImages != null ? src.OutfitImages.Count : 0))
    
    // Count total sizes
.ForMember(dest => dest.TotalSizes, 
        opt => opt.MapFrom(src => src.OutfitSizes != null ? src.OutfitSizes.Count : 0))
    
    // Count available sizes (in stock)
    .ForMember(dest => dest.AvailableSizes, 
        opt => opt.MapFrom(src => src.OutfitSizes != null 
            ? src.OutfitSizes.Count(s => s.StockQuantity > 0) : 0))
    
    // Get primary image URL (lowest sort order)
    .ForMember(dest => dest.PrimaryImageUrl, 
      opt => opt.MapFrom(src => src.OutfitImages != null && src.OutfitImages.Any()
       ? src.OutfitImages.OrderBy(img => img.SortOrder).FirstOrDefault()!.ImageUrl
       : null))
    
    // Calculate average rating
    .ForMember(dest => dest.AverageRating, 
opt => opt.MapFrom(src => src.Reviews != null && src.Reviews.Any()
  ? src.Reviews.Average(r => (double?)r.Rating)
   : null))
    
    // Count reviews
    .ForMember(dest => dest.TotalReviews, 
      opt => opt.MapFrom(src => src.Reviews != null ? src.Reviews.Count : 0));
```

#### OutfitDetailDto Mapping
```csharp
CreateMap<Outfit, OutfitDetailDto>()
    .ForMember(dest => dest.CategoryName, ...)
    .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.OutfitImages))
    .ForMember(dest => dest.Sizes, opt => opt.MapFrom(src => src.OutfitSizes))
    .ForMember(dest => dest.AverageRating, ...)
    .ForMember(dest => dest.TotalReviews, ...)
    .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => src.OutfitAttribute));

// Nested mappings
CreateMap<OutfitImage, OutfitImageInfo>();
CreateMap<OutfitSize, OutfitSizeInfo>();
CreateMap<OutfitAttribute, OutfitAttributeInfo>();
```

---

## ?? Service Layer Features

### IOutfitService Methods:

| Method | Description | Returns |
|--------|-------------|---------|
| `GetAllAsync()` | Get all outfits | `IEnumerable<OutfitResponseDto>` |
| `GetAvailableOutfitsAsync()` | Get available outfits | `IEnumerable<OutfitResponseDto>` |
| `GetOutfitsByCategoryIdAsync(int)` | Filter by category | `IEnumerable<OutfitResponseDto>` |
| `GetByIdAsync(int)` | Get basic info | `OutfitResponseDto?` |
| `GetDetailByIdAsync(int)` | Get full details | `OutfitDetailDto?` |
| `SearchAsync(string)` | Search outfits | `IEnumerable<OutfitResponseDto>` |
| `AddAsync(CreateOutfitDto)` | Create outfit | `int` (new ID) |
| `UpdateAsync(int, UpdateOutfitDto)` | Update outfit | `bool` |
| `DeleteAsync(int)` | Delete outfit | `bool` |

### Business Logic:
- ? Validates category existence before create/update
- ? Auto-sets CreatedAt timestamp on creation
- ? Supports partial updates (null values ignored)
- ? Returns outfit ID on successful creation
- ? Multi-field search functionality
- ? Handles navigation property loading

---

## ?? Testing Examples

### Get Available Outfits (cURL)
```bash
curl -X GET "https://localhost:7000/api/outfit/available"
```

### Search Outfits (cURL)
```bash
curl -X GET "https://localhost:7000/api/outfit/search?q=summer"
```

### Get Outfit Details (cURL)
```bash
curl -X GET "https://localhost:7000/api/outfit/1/detail"
```

### Create Outfit (cURL)
```bash
curl -X POST "https://localhost:7000/api/outfit" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "categoryId": 1,
    "name": "Summer Dress",
    "type": "Dress",
    "gender": "Female",
    "region": "Western",
    "isLimited": false,
 "status": "Available",
    "baseRentalPrice": 150.00
  }'
```

### Update Outfit (cURL)
```bash
curl -X PUT "https://localhost:7000/api/outfit/1" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Low Stock",
    "baseRentalPrice": 175.00
  }'
```

### Delete Outfit (cURL)
```bash
curl -X DELETE "https://localhost:7000/api/outfit/1" \
-H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

## ?? Response Examples

### Basic Outfit Response
```json
{
  "success": true,
  "data": {
    "outfitId": 1,
    "categoryId": 1,
    "name": "Summer Dress",
    "type": "Dress",
 "gender": "Female",
    "region": "Western",
    "isLimited": false,
    "status": "Available",
    "baseRentalPrice": 150.00,
    "createdAt": "2024-01-15T10:30:00Z",
    "categoryName": "Dresses",
    "totalImages": 5,
    "totalSizes": 4,
    "availableSizes": 3,
    "primaryImageUrl": "https://cdn.example.com/dress1.jpg",
    "averageRating": 4.5,
    "totalReviews": 23
  }
}
```

### Search Results
```json
{
  "success": true,
  "data": [
  {
      "outfitId": 1,
 "name": "Summer Dress",
      "baseRentalPrice": 150.00,
      "averageRating": 4.5,
      ...
    },
    {
      "outfitId": 5,
   "name": "Summer Blouse",
      "baseRentalPrice": 80.00,
      "averageRating": 4.2,
      ...
 }
  ],
  "count": 2
}
```

---

## ?? Status Codes

| Code | Meaning | When |
|------|---------|------|
| 200 | OK | Successful GET, PUT, DELETE |
| 201 | Created | Successful POST |
| 400 | Bad Request | Invalid input or category not found |
| 401 | Unauthorized | Missing/invalid token |
| 404 | Not Found | Outfit doesn't exist |
| 500 | Server Error | Internal error |

---

## ?? Best Practices Followed

? **Repository Pattern** - Using Unit of Work for data access  
? **DTO Pattern** - Separate models for different operations  
? **AutoMapper** - Clean object mapping with complex projections  
? **Validation** - Data annotations on input DTOs  
? **Authorization** - Protected write operations  
? **Error Handling** - Comprehensive error responses  
? **RESTful Design** - Proper HTTP methods and endpoints  
? **Separation of Concerns** - Layered architecture  
? **Navigation Properties** - Efficient loading of related data  
? **Search Functionality** - Multi-field search support  
? **Aggregated Data** - Calculated fields in responses  

---

## ?? Build Status
? **Build Successful** - All files compile without errors

---

## ?? Advanced Features

### 1. **Aggregated Data**
Response DTOs automatically include:
- Total number of images
- Total number of sizes
- Number of available sizes (in stock)
- Primary image URL
- Average rating from reviews
- Total review count

### 2. **Detailed View**
`/api/outfit/{id}/detail` returns:
- Complete outfit information
- All images with sort order
- All sizes with stock info
- Review statistics
- Full attributes (material, occasion, etc.)

### 3. **Search Functionality**
Search across multiple fields:
- Outfit name
- Type (e.g., "dress", "suit")
- Gender (e.g., "female", "male", "unisex")
- Region (e.g., "western", "asian")

### 4. **Filtering**
- By category
- By availability status
- By availability (in stock)

### 5. **Partial Updates**
Update only the fields you need:
```json
{
  "baseRentalPrice": 199.99
}
```
Other fields remain unchanged.

---

## ?? Configuration

### Service Registration
Registered in `Program.cs`:
```csharp
builder.Services.AddScoped<IOutfitService, OutfitService>();
```

### Dependencies
- ? EXE201.Repository
- ? AutoMapper
- ? Microsoft.AspNetCore.Mvc
- ? Microsoft.AspNetCore.Authorization

---

## ?? Related Entities

The Outfit entity has relationships with:
- **Category** (many-to-one)
- **OutfitImages** (one-to-many)
- **OutfitSizes** (one-to-many)
- **OutfitAttribute** (one-to-one)
- **Reviews** (one-to-many)
- **Wishlists** (one-to-many)

All navigation properties are properly mapped and available in the DetailDto.

---

## ?? Use Cases

### For Customers:
1. Browse all available outfits
2. Filter by category
3. Search by keywords
4. View detailed outfit information
5. Check availability (sizes in stock)
6. Read reviews and ratings

### For Admins:
1. Create new outfits
2. Update outfit information
3. Update pricing
4. Change availability status
5. Delete outfits
6. Manage inventory

---

## ?? Notes

- CreatedAt is automatically set to UTC time on creation
- Category must exist before creating an outfit
- Deleting an outfit cascades to related entities
- Search is case-sensitive (can be modified for case-insensitive)
- Primary image is determined by the lowest SortOrder value
- Average rating is calculated from all reviews
- Available sizes count only includes sizes with StockQuantity > 0

---

## ?? Future Enhancements

Potential improvements:
- Pagination for large result sets
- Advanced filtering (price range, rating range)
- Sorting options (by price, rating, date)
- Case-insensitive search
- Fuzzy search
- Image upload functionality
- Bulk operations
- Soft delete support

---

This implementation provides a complete, production-ready CRUD API for the Outfit entity with advanced features for both customers and administrators! ??
