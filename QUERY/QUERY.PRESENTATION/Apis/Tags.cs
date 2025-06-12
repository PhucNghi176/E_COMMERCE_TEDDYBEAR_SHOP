using Carter;
using CONTRACT.CONTRACT.PRESENTATION.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Q = QUERY.CONTRACT.Services.Tags.Query;

namespace QUERY.PRESENTATION.Apis;
public class Tags : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "api/{version:apiVersion}/tags";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Tags").MapGroup(BaseUrl).HasApiVersion(1);
        gr1.MapGet("", GetTagsAsync)
            .WithName("GetTags")
            .Produces<IReadOnlyList<QUERY.CONTRACT.Services.Tags.Response.TagResponse>>()
            .WithTags("Tags")
            .WithSummary("Get all tags");
    }

    private static async Task<IResult> GetTagsAsync(
        [FromRoute]Q.GetTagsQuery query, ISender sender)
    {
        var result = await sender.Send(query);
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}