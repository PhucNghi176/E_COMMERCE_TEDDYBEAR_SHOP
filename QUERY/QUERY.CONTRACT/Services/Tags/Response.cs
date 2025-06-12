namespace QUERY.CONTRACT.Services.Tags;
public class Response
{
    public record TagResponse(
        string Id,
        string Name
    );
}