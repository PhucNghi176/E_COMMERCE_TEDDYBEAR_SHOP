using Carter;
using CONTRACT.CONTRACT.PRESENTATION.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace COMMAND.PRESENTATION.Apis;
public class ImageApi : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "api/v{version:apiVersion}/images";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Images").MapGroup(BaseUrl).HasApiVersion(1);
        gr1.MapPost("", UploadImageAsync).DisableAntiforgery();
    }

    private static async Task<IResult> UploadImageAsync(
         IFormFile file, ISender sender)
    {
        var result = await sender.Send(
            new CONTRACT.Services.Images.Commands.UploadImageCommand(file));
        return result.IsSuccess ? Results.Ok(result) : HandlerFailure(result);
    }
}