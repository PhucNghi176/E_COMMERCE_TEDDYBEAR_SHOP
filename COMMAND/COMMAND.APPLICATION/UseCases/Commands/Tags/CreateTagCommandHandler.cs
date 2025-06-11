using CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using CONTRACT.CONTRACT.DOMAIN.Entities;
using CONTRACT.CONTRACT.DOMAIN.Exceptions;

namespace COMMAND.APPLICATION.UseCases.Commands.Tags;
internal sealed class CreateTagCommandHandler(IRepositoryBase<Tag, int> tagRepositoryBase)
    : ICommandHandler<CONTRACT.Services.Tags.Commands.CreateTagCommand>
{
    public async Task<Result> Handle(CONTRACT.Services.Tags.Commands.CreateTagCommand request,
        CancellationToken cancellationToken)
    {
        var tag = await tagRepositoryBase.FindSingleAsync(x => x.Name.Equals(request.Name), cancellationToken);
        if (tag != null)
            throw new TagException.TagAlreadyExistsException(request.Name);
        tagRepositoryBase.Add(new Tag
        {
            Name = request.Name
        });

        return Result.Success();
    }
}