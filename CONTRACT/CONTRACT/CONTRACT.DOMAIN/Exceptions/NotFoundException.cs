namespace CONTRACT.CONTRACT.DOMAIN.Exceptions;
public abstract class NotFoundException(string title, string? message) : DomainException(title, message);