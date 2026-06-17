using Microsoft.AspNetCore.Http;

namespace Application.Dtos.Common
{
    /// <summary>
    /// Response DTO for file upload operations
    /// </summary>
    public record UploadPhotoResponse(
        bool Success,
        string? FileUrl,
        string? ErrorMessage = null);

    /// <summary>
    /// Request DTO for file uploads (used in multipart/form-data)
    /// </summary>
    public class UploadPhotoRequest
    {
        public IFormFile? Photo { get; set; }
    }
}
