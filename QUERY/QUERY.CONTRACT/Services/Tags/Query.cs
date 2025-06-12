using CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;

namespace QUERY.CONTRACT.Services.Tags;
public static class Query
{
    public record GetTagsQuery() : IQuery<IReadOnlyList<Response.TagResponse>>;
}