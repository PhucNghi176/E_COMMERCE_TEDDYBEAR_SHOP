using CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using CONTRACT.CONTRACT.DOMAIN.Entities;
using CONTRACT.CONTRACT.DOMAIN.Exceptions;

namespace COMMAND.APPLICATION.UseCases.Commands.Products;
internal sealed class DeleteProductCommandHandler(IRepositoryBase<Product, int> repositoryBase)
    : ICommandHandler<CONTRACT.Services.Products.Commands.DeleteProductCommand>
{
    public async Task<Result> Handle(CONTRACT.Services.Products.Commands.DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await repositoryBase.FindSingleAsync(x => x.Id.Equals(request.Id), cancellationToken) ??
                      throw new ProductException.ProductNotFoundException();
        repositoryBase.Remove(product);
        return Result.Success();
    }
}