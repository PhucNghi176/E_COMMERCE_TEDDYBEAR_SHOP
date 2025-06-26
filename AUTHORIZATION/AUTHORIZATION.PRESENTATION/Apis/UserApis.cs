using AUTHORIZATION.CONTRACT.Identity;
using Carter;
using CONTRACT.CONTRACT.PRESENTATION.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AUTHORIZATION.PRESENTATION.Apis;
public class UserApis : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "api/v{apiversion:apiVersion}/users";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Users")
            .MapGroup(BaseUrl)
            .HasApiVersion(1);
        gr1.MapPost("login", Login);
    }

    private static async Task<IResult> Login(ISender sender, [FromBody] Query.LoginQuery query)
    {
        var result = await sender.Send(query);
        return result.IsSuccess ? Results.Ok(result) : HandlerFailure(result);
    }
}