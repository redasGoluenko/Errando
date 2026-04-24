namespace Errando.Services;

public interface IImageStorageService
{
    /// <summary>
    /// Uploads an image file and returns the URL/path to access it
    /// </summary>
    Task<string?> UploadImageAsync(Stream imageStream, string fileName, string contentType);

    /// <summary>
    /// Deletes an image file
    /// </summary>
    Task<bool> DeleteImageAsync(string imageUrl);
}
