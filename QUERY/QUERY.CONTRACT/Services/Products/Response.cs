namespace QUERY.CONTRACT.Services.Products;
public static class Response
{
    public record ProductResponse(
        int Id,
        string Name,
        int Quantity,
        decimal Price,
        string[] Tags,
        string[] ImgUrl,
        string[] Color,
        DateTimeOffset CreatedOnUtc);
}