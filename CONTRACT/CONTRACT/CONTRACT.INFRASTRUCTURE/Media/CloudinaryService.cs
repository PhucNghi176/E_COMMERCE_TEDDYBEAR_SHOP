using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CONTRACT.CONTRACT.APPLICATION.Abstractions;
using Microsoft.AspNetCore.Http;

namespace CONTRACT.CONTRACT.INFRASTRUCTURE.Media;
public class CloudinaryService(Cloudinary cloudinary) : IMediaService
{
    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0) throw new ArgumentException("File is empty or null.", nameof(file));
        if (!IsImageFile(file)) throw new ArgumentException("File is not a valid image.", nameof(file));

        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream)
        };
        var uploadResult = await cloudinary.UploadAsync(uploadParams);
        return uploadResult.SecureUrl.ToString();
    }

    private bool IsImageFile(IFormFile file)
    {
        // This is a basic check. For more robust validation, consider using a library like MimeDetective
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedExtensions.Contains(fileExtension);
    }
}