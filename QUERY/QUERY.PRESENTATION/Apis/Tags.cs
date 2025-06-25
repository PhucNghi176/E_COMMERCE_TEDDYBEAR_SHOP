using Carter;
using CONTRACT.CONTRACT.PRESENTATION.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using QUERY.CONTRACT.Services.Tags;

namespace QUERY.PRESENTATION.Apis;
public class Tags : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "api/v{version:apiVersion}/tags";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Tags").MapGroup(BaseUrl).HasApiVersion(1);
        gr1.MapGet("", GetTagsAsync)
            .WithName("GetTags")
            .Produces<IReadOnlyList<Response.TagResponse>>()
            .WithTags("Tags")
            .WithSummary("Get all tags");
    }

    private static async Task<IResult> GetTagsAsync(ISender sender)
    {
        var query = new Query.GetTagsQuery();
        var result = await sender.Send(query);
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}