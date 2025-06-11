using Microsoft.AspNetCore.Http;

namespace CONTRACT.CONTRACT.APPLICATION.Abstractions;

public interface IMediaService
{
    Task<string> UploadImageAsync(IFormFile file);
}