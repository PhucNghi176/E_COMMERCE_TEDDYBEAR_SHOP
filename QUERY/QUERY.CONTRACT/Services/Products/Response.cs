namespace QUERY.CONTRACT.Services.Products;
public static class Response
{
    public record ProductResponse(
        int Id,
        string Name,
        int Quantity,
        decimal Price,
        string size,
        TagResponse[] Tags,
        string PrimaryImageUrl,
        string[] Color,
        DateTimeOffset CreatedOnUtc);

    public record TagResponse(
        int Id,
        string Name);
}