namespace AUTHORIZATION.CONTRACT.Identity;
public static class Response
{
    public record Authenticated(string Token, string RefreshToken);
}