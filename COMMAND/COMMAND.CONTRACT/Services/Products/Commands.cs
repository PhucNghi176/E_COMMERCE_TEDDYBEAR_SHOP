using CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;

namespace COMMAND.CONTRACT.Services.Products;
public static class Commands
{
    public record CreateProductCommand(
        string? Name,
        string? Size,
        string[]? Color,
        string[]? ImgUrl,
        int[]? TagIds,
        string PrimaryImgUrl,
        int Quantity = 1,
        decimal Price = 0.1m
    ) : ICommand;
}