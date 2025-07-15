using Carter;
using COMMAND.CONTRACT.Services.Products;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
using CONTRACT.CONTRACT.PRESENTATION.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace COMMAND.PRESENTATION.Apis;
public class ProductApi : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "api/v{version:apiVersion}/products";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Products").MapGroup(BaseUrl).HasApiVersion(1);
        gr1.MapPost("", CreateProduct);
        gr1.MapDelete("", DeleteProduct);
    }

    private static async Task<IResult> CreateProduct(
        [FromBody] Commands.CreateProductCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> DeleteProduct(
        [FromBody] Commands.DeleteProductCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}