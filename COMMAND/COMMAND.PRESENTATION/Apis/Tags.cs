using Carter;
using COMMAND.CONTRACT.Services.Tags;
using CONTRACT.CONTRACT.PRESENTATION.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace COMMAND.PRESENTATION.Apis;
public class Tags : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "api/{version:apiVersion}/tags";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Tags").MapGroup(BaseUrl).HasApiVersion(1);
        _ = gr1.MapPost("", CreateTagAsync);

    }

    private static async Task<IResult> CreateTagAsync(
        [FromBody] Commands.CreateTagCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}