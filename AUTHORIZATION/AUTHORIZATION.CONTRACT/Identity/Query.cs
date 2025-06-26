using CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;

namespace AUTHORIZATION.CONTRACT.Identity;
public static class Query
{
    public record LoginQuery(string Email, string Password) : IQuery<Response.Authenticated>;
}