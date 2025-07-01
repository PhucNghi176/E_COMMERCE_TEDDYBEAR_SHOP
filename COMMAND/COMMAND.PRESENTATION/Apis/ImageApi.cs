using Carter;
using COMMAND.CONTRACT.Services.Images;
using CONTRACT.CONTRACT.PRESENTATION.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace COMMAND.PRESENTATION.Apis;
public class ImageApi : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "api/v{version:apiVersion}/images";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Images").MapGroup(BaseUrl).HasApiVersion(1);
        gr1.MapPost("upload", UploadImageAsync).DisableAntiforgery().RequireAuthorization();
    }

    private static async Task<IResult> UploadImageAsync(
        IFormFile file, ISender sender)
    {
        var result = await sender.Send(
            new Commands.UploadImageCommand(file));
        return result.IsSuccess ? Results.Ok(result) : HandlerFailure(result);
    }
}