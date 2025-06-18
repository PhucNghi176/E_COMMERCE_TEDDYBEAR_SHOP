using CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;
using Microsoft.AspNetCore.Http;

namespace COMMAND.CONTRACT.Services.Images;
public static class Commands
{
    public record UploadImageCommand(
        IFormFile File
    ) : ICommand;
}