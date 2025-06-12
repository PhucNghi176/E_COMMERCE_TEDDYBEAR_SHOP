using CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using CONTRACT.CONTRACT.DOMAIN.Entities;
using CONTRACT.CONTRACT.DOMAIN.Exceptions;
using Microsoft.EntityFrameworkCore;
using QUERY.CONTRACT.Services.Tags;

namespace QUERY.APPLICATION.UseCases.Queries.Tags;
internal sealed class GetTagsQueryHandler(IRepositoryBase<Tag, int> repositoryBase)
    : IQueryHandler<Query.GetTagsQuery, IReadOnlyList<Response.TagResponse>>
{
    public async Task<Result<IReadOnlyList<Response.TagResponse>>> Handle(Query.GetTagsQuery request,
        CancellationToken cancellationToken)
    {
        var tags = await repositoryBase.FindAll().ToListAsync(cancellationToken);
        if (tags.Count == 0)
            throw new TagException.TagNotFoundException();
        var tagResponses = tags.Select(tag => new Response.TagResponse(tag.Id.ToString(), tag.Name)).ToList();
        return Result.Success<IReadOnlyList<Response.TagResponse>>(tagResponses);
    }
}