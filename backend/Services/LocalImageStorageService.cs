namespace Errando.Services;

public class LocalImageStorageService : IImageStorageService
{
    private readonly string _uploadDirectory;
    private readonly ILogger<LocalImageStorageService> _logger;
    private const long MaxFileSize = 1 * 1024 * 1024; // 1 MB

    public LocalImageStorageService(IWebHostEnvironment environment, ILogger<LocalImageStorageService> logger)
    {
        _logger = logger;
        
        // Use WebRootPath if available, otherwise use a default uploads directory
        var basePath = environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        _uploadDirectory = Path.Combine(basePath, "uploads", "task-photos");
        
        // Create directory if it doesn't exist
        if (!Directory.Exists(_uploadDirectory))
        {
            Directory.CreateDirectory(_uploadDirectory);
            _logger.LogInformation($"Created upload directory: {_uploadDirectory}");
        }
    }

    public async Task<string?> UploadImageAsync(Stream imageStream, string fileName, string contentType)
    {
        try
        {
            // Validate file size
            if (imageStream.Length > MaxFileSize)
            {
                _logger.LogWarning($"Image file too large: {imageStream.Length} bytes (max: {MaxFileSize})");
                return null;
            }

            // Validate content type
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowedTypes.Contains(contentType))
            {
                _logger.LogWarning($"Invalid image content type: {contentType}");
                return null;
            }

            // Generate unique filename
            var extension = GetExtensionFromContentType(contentType);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_uploadDirectory, uniqueFileName);

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            // Return relative URL path
            var relativeUrl = $"/uploads/task-photos/{uniqueFileName}";
            _logger.LogInformation($"Image uploaded successfully: {relativeUrl}");
            return relativeUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading image: {fileName}");
            return null;
        }
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            // Extract filename from URL (e.g., "/uploads/task-photos/guid.jpg" -> "guid.jpg")
            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(_uploadDirectory, fileName);

            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                _logger.LogInformation($"Image deleted successfully: {imageUrl}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting image: {imageUrl}");
            return false;
        }
    }

    private string GetExtensionFromContentType(string contentType)
    {
        return contentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            _ => ".jpg"
        };
    }
}
