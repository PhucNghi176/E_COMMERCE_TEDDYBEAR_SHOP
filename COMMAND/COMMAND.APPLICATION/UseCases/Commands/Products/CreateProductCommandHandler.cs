using CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using CONTRACT.CONTRACT.DOMAIN.Entities;

namespace COMMAND.APPLICATION.UseCases.Commands.Products;
public sealed class
    CreateProductCommandHandler(IRepositoryBase<Product, int> repositoryBase)
    : ICommandHandler<CONTRACT.Services.Products.Commands.CreateProductCommand>
{
    public async Task<Result> Handle(CONTRACT.Services.Products.Commands.CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Size = request.Size,
            Quantity = request.Quantity,
            Price = request.Price,
            Color = request.Color,
            ImgUrl = request.ImgUrl,
        };
        if (request.TagIds is not null)
        {
            foreach (var tagId in request.TagIds)
            {
                product.ProductTags.Add(new ProductTag
                {
                    Product = product,
                    TagId = tagId
                });
            }
        }

        repositoryBase.Add(product);
        return Result.Success();
    }
}