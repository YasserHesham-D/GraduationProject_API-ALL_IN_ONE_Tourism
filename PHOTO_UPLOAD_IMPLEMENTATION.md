# Photo Upload API Implementation Summary

## Overview
The TourismG API now supports file uploads from Flutter mobile applications, allowing users to upload photos for services, places, user profiles, and reviews.

## Implemented Features

### 1. File Upload Service (`TourismG_API/Services/FileUploadService.cs`)
- **IFileUploadService** interface for dependency injection
- **Validation**: Supports JPG, PNG, WEBP, GIF, BMP, TIFF, ICO formats
- **Size limit**: 5 MB per file (configurable)
- **Unique filenames**: Uses timestamp + GUID to prevent file collisions
- **Thread-safe**: Lock mechanism for concurrent uploads

### 2. Upload Endpoints

#### Service Photo Upload
```
POST /api/provider/services/{id}/upload-photo
Authorization: Bearer <JWT_TOKEN>
Content-Type: multipart/form-data

Body:
  photo: <IFormFile>

Response (200):
{
  "success": true,
  "fileUrl": "/api/files/uploads/20260617_130059_a1b2c3d4.jpg"
}
```

#### Place Photo Upload
```
POST /api/provider/places/{id}/upload-photo
Authorization: Bearer <JWT_TOKEN>
Content-Type: multipart/form-data

Body:
  photo: <IFormFile>

Response (200):
{
  "success": true,
  "fileUrl": "/api/files/uploads/20260617_130059_b5c6d7e8.png"
}
```

#### User Avatar Upload
```
POST /api/profile/upload-avatar
Authorization: Bearer <JWT_TOKEN>
Content-Type: multipart/form-data

Body:
  photo: <IFormFile>

Response (200):
{
  "success": true,
  "fileUrl": "/api/files/uploads/20260617_130059_c9d0e1f2.jpg"
}
```

#### Review Photo Upload
```
POST /api/reviews/{id}/upload-photo
Authorization: Bearer <JWT_TOKEN>
Content-Type: multipart/form-data

Body:
  photo: <IFormFile>

Response (200):
{
  "success": true,
  "fileUrl": "/api/files/uploads/20260617_130059_d3e4f5g6.png"
}
```

### 3. File Serving Endpoint
```
GET /api/files/{*filePath}
Content-Type: image/* (auto-detected)

Example:
GET /api/files/uploads/20260617_130059_a1b2c3d4.jpg
```
- Serves uploaded files with proper MIME types
- Path traversal protection (prevents ../ attacks)
- Returns 404 if file not found

### 4. Database Support
New columns added via migration `AddPhotoUploadSupport`:
- `ServiceOfferings.UpdatedAt` (DateTime?)
- `PlaceReviews.PhotoUrl` (string?)
- `AspNetUsers.ProfileImageUrl` (string?)

### 5. DTO Models

**UploadPhotoRequest** (`Application/Dtos/Common/FileUploadDtos.cs`)
```csharp
public class UploadPhotoRequest
{
	public IFormFile? Photo { get; set; }
}
```

**UploadPhotoResponse** (`Application/Dtos/Common/FileUploadDtos.cs`)
```csharp
public record UploadPhotoResponse(
	bool Success,
	string? FileUrl,
	string? ErrorMessage = null);
```

## File Storage

### Directory Structure
```
TourismG_API/
├── wwwroot/
│   └── uploads/
│       ├── .gitkeep
│       ├── 20260617_130059_a1b2c3d4.jpg
│       ├── 20260617_130059_b5c6d7e8.png
│       └── ... (other uploaded files)
```

### File Organization
- **Location**: `wwwroot/uploads/` directory
- **Filename format**: `{yyyyMMdd_HHmmss}_{GUID}{extension}`
- **Public access**: Via `/api/files/uploads/{filename}` endpoint
- **Automatic cleanup**: On new upload, old file is deleted

## Error Handling

### Error Responses
```
400 Bad Request:
{
  "success": false,
  "fileUrl": null,
  "errorMessage": "Invalid file. Allowed formats: JPG, PNG, WEBP, GIF, BMP. Max size: 5 MB"
}

401 Unauthorized:
When JWT token is missing or invalid

404 Not Found:
When entity (service/place/user/review) doesn't exist

500 Server Error:
File system or database errors
```

## Configuration

### Dependencies
- `Microsoft.AspNetCore.Http.Abstractions` (for IFormFile)
- `Microsoft.Extensions.Hosting.Abstractions` (for IWebHostEnvironment)

### DI Registration (`Program.cs`)
```csharp
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
```

### Middleware
```csharp
app.UseStaticFiles();  // Enable static file serving
```

## Security Considerations

1. **Authentication**: All upload endpoints require valid JWT Bearer token
2. **Authorization**: Old/new image URLs compared to verify user ownership
3. **File Validation**:
   - MIME type verification
   - File extension validation
   - File size limits (5 MB max)
4. **Path Traversal Protection**: Path normalization in file serving endpoint
5. **File Deletion**: Old files automatically removed to prevent disk space issues

## Testing Endpoints

### Using cURL
```bash
# Upload service photo
curl -X POST http://localhost:5000/api/provider/services/{serviceId}/upload-photo \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -F "photo=@/path/to/image.jpg"

# Upload user avatar
curl -X POST http://localhost:5000/api/profile/upload-avatar \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -F "photo=@/path/to/avatar.png"
```

### Using Swagger/Postman
1. Navigate to `/swagger` in API UI
2. Authenticate with JWT token
3. Find upload endpoint (e.g., `/api/provider/services/{id}/upload-photo`)
4. Click "Try it out"
5. Select file and execute

## Flutter Integration

See `FLUTTER_UPLOAD_GUIDE.md` for comprehensive Flutter implementation examples including:
- Dio HTTP client setup
- Image picker integration
- Upload service creation
- Error handling patterns
- Complete working examples
- Background upload strategies

## Performance Notes

- **Upload speed**: Limited by file size and network speed
- **Concurrent uploads**: Thread-safe via lock mechanism
- **Disk usage**: Monitor wwwroot/uploads directory size
- **Image compression**: Recommended on Flutter side before upload

## Future Enhancements

- Image resizing (multiple dimensions for thumbnails)
- Image compression on server
- Async file cleanup task
- Cloud storage integration (Azure Blob, AWS S3)
- Virus scanning for uploaded files
- Rate limiting per user
- Upload progress tracking via WebSockets
