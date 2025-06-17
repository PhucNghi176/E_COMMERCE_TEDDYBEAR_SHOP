using System.Linq.Expressions;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
using CONTRACT.CONTRACT.CONTRACT.Enumerations;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using CONTRACT.CONTRACT.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;
using QUERY.CONTRACT.Services.Products;

namespace QUERY.APPLICATION.UseCases.Queries.Products;
internal sealed class GetProductsQueryHandler(IRepositoryBase<Product, int> repositoryBase)
    : IQueryHandler<Query.GetProducts, PagedResult<Response.ProductResponse>>
{
    public async Task<Result<PagedResult<Response.ProductResponse>>> Handle(Query.GetProducts request,
        CancellationToken cancellationToken)
    {
        var query = repositoryBase.FindAll();

        if (!string.IsNullOrWhiteSpace(request.searchTerm))
        {
            query = query.Where(p =>
                p.Name.Contains(request.searchTerm) ||
                p.Color.Contains(request.searchTerm) ||
                p.ProductTags.Any(x => x.Tag.Name.Contains(request.searchTerm))
            );
        }

        query = query.Include(x => x.ProductTags);

        query = request.sortOrder == SortOrder.Descending
            ? query.OrderByDescending(GetSortProperty(request))
            : query.OrderBy(GetSortProperty(request));

        var products = await PagedResult<Product>.CreateAsync(query, request.pageIndex, request.pageSize);
        var mappedProducts = products.Items.Select(product => new Response.ProductResponse(
            product.Id,
            product.Name,
            product.Quantity,
            product.Price,
            product.Size,
            product.ProductTags.Select(tag => new Response.TagResponse(
                tag.Tag.Id,
                tag.Tag.Name)).ToArray(),
            product.ImgUrl.ToArray(),
            product.Color.ToArray(),
            product.CreatedOnUtc)).ToList();

        return new PagedResult<Response.ProductResponse>(mappedProducts, products.PageIndex,
            products.PageSize, products.TotalCount);
    }


    private static Expression<Func<Product, object>> GetSortProperty(Query.GetProducts request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => projection => projection.Name,
            _ => projection => projection.CreatedOnUtc
        };
    }
}