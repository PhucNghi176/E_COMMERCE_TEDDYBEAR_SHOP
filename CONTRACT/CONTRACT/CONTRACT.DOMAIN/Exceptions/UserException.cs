namespace CONTRACT.CONTRACT.DOMAIN.Exceptions;
public static class UserException
{
    public class NotFound(string title = "Not found", string? message = "User not found")
        : NotFoundException(title, message);

    public class PasswordMismatch(
        string title = "Password mismatch",
        string? message = "The provided password does not match the user's password")
        : DomainException(title, message);
}