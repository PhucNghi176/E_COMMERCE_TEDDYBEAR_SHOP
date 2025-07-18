﻿using Carter;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
using CONTRACT.CONTRACT.CONTRACT.Extensions;
using CONTRACT.CONTRACT.PRESENTATION.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using QUERY.CONTRACT.Services.Products;

namespace QUERY.PRESENTATION.Apis;
public class ProductApi : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "api/v{version:apiVersion}/products";
    private const string ProductCacheTag = "products";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Products").MapGroup(BaseUrl).HasApiVersion(1);
        gr1.MapGet("", GetProductsAsync)
            .WithName("GetProducts")
            .Produces<PagedResult<Response.ProductResponse>>()
            .WithTags("Products")
            .WithSummary("Get all products")
            .CacheOutput(policy => policy
                .Tag(ProductCacheTag)
                .Expire(TimeSpan.FromMinutes(5)));
        gr1.MapGet("refresh-cache", RefreshProductCache)
            .WithName("RefreshProductCache")
            .Produces(StatusCodes.Status200OK)
            .WithTags("Products")
            .WithSummary("Manually invalidate products cache")
            .RequireAuthorization();
    }

    private static async Task<IResult> GetProductsAsync(
        ISender sender,
        string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        string? tag = null,
        int pageIndex = 1,
        int pageSize = 10
    )
    {
        var result = await sender.Send(new Query.GetProducts(searchTerm, sortColumn,
            SortOrderExtension.ConvertStringToSortOrder(sortOrder), tag, pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> RefreshProductCache(IOutputCacheStore cacheStore,
        CancellationToken cancellationToken)
    {
        await cacheStore.EvictByTagAsync(ProductCacheTag, cancellationToken);
        return Results.Ok(new { message = "Products cache refreshed successfully" });
    }
}