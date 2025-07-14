using System.Linq.Expressions;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
using CONTRACT.CONTRACT.CONTRACT.Enumerations;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using CONTRACT.CONTRACT.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;
using QUERY.CONTRACT.Services.Products;

namespace QUERY.APPLICATION.UseCases.Queries.Products;
public sealed class GetProductsQueryHandler(IRepositoryBase<Product, int> repositoryBase)
    : IQueryHandler<Query.GetProducts, PagedResult<Response.ProductResponse>>
{
    public async Task<Result<PagedResult<Response.ProductResponse>>> Handle(Query.GetProducts request,
        CancellationToken cancellationToken)
    {
        // Fixed: Use proper Include/ThenInclude pattern
        var query = repositoryBase.FindAll(null, p => p.ProductTags);
        query = query.Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag);

        // Optimized: Use EF.Functions.Like for better database performance on text search
        if (!string.IsNullOrWhiteSpace(request.searchTerm))
        {
            var searchTermLower = request.searchTerm.ToLower();
            query = query.Where(p =>
                EF.Functions.Like(p.Name.ToLower(), $"%{searchTermLower}%") ||
                //  EF.Functions.Like(p.Color.ToLower(), $"%{searchTermLower}%") ||
                p.ProductTags.Any(x => EF.Functions.Like(x.Tag.Name.ToLower(), $"%{searchTermLower}%"))
            );
        }

        // Optimized: Improve tag filtering performance
        if (request.Tag is not null)
        {
            var listTags = request.Tag.Split("&&", StringSplitOptions.RemoveEmptyEntries)
                .Select(tag => tag.Trim().ToLower())
                .ToHashSet(); // Use HashSet for O(1) lookups

            // Filter products that contain ALL specified tags
            query = query.Where(p =>
                listTags.All(tagName =>
                    p.ProductTags.Any(pt => pt.Tag.Name.ToLower() == tagName)
                )
            );
        }

        // Apply sorting before pagination for better performance
        query = request.sortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request))
            : query.OrderBy(GetSortProperty(request));

        // Execute pagination
        var products = await PagedResult<Product>.CreateAsync(query, request.pageIndex, request.pageSize);

        // Optimized: Use more efficient mapping without multiple enumeration
        var mappedProducts = products.Items.Select(product => new Response.ProductResponse(
            product.Id,
            product.Name,
            product.Quantity,
            product.Price,
            product.Size,
            product.ProductTags.Select(tag => new Response.TagResponse(
                tag.Tag.Id,
                tag.Tag.Name)).ToArray(),
            product.PrimaryImgUrl,
            ParseColorArray(product.Color), // Optimize color parsing
            product.CreatedOnUtc)).ToList();

        return new PagedResult<Response.ProductResponse>(mappedProducts, products.PageIndex,
            products.PageSize, products.TotalCount);
    }

    private static Expression<Func<Product, object>> GetSortProperty(Query.GetProducts request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => projection => projection.Name,
            "price" => projection => projection.Price,
            "quantity" => projection => projection.Quantity,
            "createdon" => projection => projection.CreatedOnUtc,
            _ => projection => projection.CreatedOnUtc
        };
    }

    // Optimized: Add efficient color parsing method
    private static string[] ParseColorArray(string[] colorString)
    {
        // Assuming color is stored as comma-separated values
        // return colorString.Split(',', StringSplitOptions.RemoveEmptyEntries)
        //                   .Select(c => c.Trim())
        //                   .ToArray();
        return colorString ?? []; // Return empty array if null
    }
}