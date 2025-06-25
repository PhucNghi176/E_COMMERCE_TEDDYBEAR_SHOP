using CONTRACT.CONTRACT.APPLICATION.Abstractions;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
using CONTRACT.CONTRACT.DOMAIN.Exceptions;

namespace COMMAND.APPLICATION.UseCases.Commands.Images;
internal sealed class UploadImageCommandHandler(IMediaService mediaService)
    : ICommandHandler<CONTRACT.Services.Images.Commands.UploadImageCommand>
{
    public async Task<Result> Handle(CONTRACT.Services.Images.Commands.UploadImageCommand request,
        CancellationToken cancellationToken)
    {
        var result = await mediaService.UploadImageAsync(request.File);
        if (result is null) throw new ImageUploadFailException();

        return Result.Success(result);
    }
}