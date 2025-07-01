using CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;

namespace QUERY.CONTRACT.Services.Products;
public static class Query
{
    public record GetProducts(
        string? searchTerm = null,
        string? SortColumn = null,
        string? sortOrder = null,
        string? Tag = null,
        int pageIndex = 1,
        int pageSize = 10) : IQuery<PagedResult<Response.ProductResponse>>;
}