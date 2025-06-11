namespace CONTRACT.CONTRACT.DOMAIN.Exceptions;
public abstract class AlreadyExistedException(string title, string message) : DomainException(title, message);