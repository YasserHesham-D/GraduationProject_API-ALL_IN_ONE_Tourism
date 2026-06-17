using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Presentation.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(IFormFile file, string uploadDirectory = "uploads");
        bool ValidateFile(IFormFile file, long maxSizeBytes = 5242880); // 5 MB default
        void DeleteFile(string filePath);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly string _webRootPath;
        private readonly object _uploadLock = new object();

        // Allowed MIME types for images
        private static readonly HashSet<string> AllowedMimeTypes = new()
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/webp",
            "image/gif",
            "image/bmp",
            "image/tiff",
            "image/x-icon"
        };

        // Allowed file extensions
        private static readonly HashSet<string> AllowedExtensions = new()
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp", ".tiff", ".ico"
        };

        public FileUploadService(IWebHostEnvironment environment)
        {
            _webRootPath = environment.WebRootPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
        }

        /// <summary>
        /// Validates if file meets size and type requirements
        /// </summary>
        public bool ValidateFile(IFormFile file, long maxSizeBytes = 5242880)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > maxSizeBytes)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return false;

            if (!AllowedMimeTypes.Contains(file.ContentType?.ToLowerInvariant() ?? string.Empty))
                return false;

            return true;
        }

        /// <summary>
        /// Uploads a file to the specified directory and returns the relative URL
        /// </summary>
        public async Task<string> UploadFileAsync(IFormFile file, string uploadDirectory = "uploads")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null", nameof(file));

            if (!ValidateFile(file))
                throw new InvalidOperationException("File validation failed. Check file type and size (max 5 MB).");

            // Create directory if it doesn't exist
            var fullUploadPath = Path.Combine(_webRootPath, uploadDirectory);

            lock (_uploadLock)
            {
                if (!Directory.Exists(fullUploadPath))
                {
                    Directory.CreateDirectory(fullUploadPath);
                }
            }

            // Generate unique filename: timestamp + guid + original extension
            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(fullUploadPath, uniqueFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative URL path (without leading slash for flexibility)
            return $"/api/files/{uploadDirectory}/{uniqueFileName}";
        }

        /// <summary>
        /// Deletes a file if it exists
        /// </summary>
        public void DeleteFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            try
            {
                // Support both stored URL (e.g. /api/files/uploads/xyz.jpg) and relative paths
                var relativePath = filePath;
                if (relativePath.StartsWith("/api/files/", StringComparison.OrdinalIgnoreCase))
                {
                    relativePath = relativePath.Substring("/api/files/".Length);
                }

                relativePath = relativePath.TrimStart('/','\\').Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                var candidateFullPath = Path.Combine(_webRootPath, relativePath);
                var fullPath = Path.GetFullPath(candidateFullPath);
                var webroot_normalized = Path.GetFullPath(_webRootPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot"));

                if (!fullPath.StartsWith(webroot_normalized, StringComparison.OrdinalIgnoreCase))
                    return;

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw - file deletion is not critical
                System.Console.WriteLine($"Error deleting file {filePath}: {ex.Message}");
            }
        }
    }
}
