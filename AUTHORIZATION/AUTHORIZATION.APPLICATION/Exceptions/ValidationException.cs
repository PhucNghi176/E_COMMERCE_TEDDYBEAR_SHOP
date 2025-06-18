using CONTRACT.CONTRACT.DOMAIN.Exceptions;

namespace AUTHORIZATION.APPLICATION.Exceptions;
public sealed class ValidationException(IReadOnlyCollection<ValidationError> errors)
    : DomainException("Validation Failure", "One or more validation errors occurred")
{
    public IReadOnlyCollection<ValidationError> Errors { get; } = errors;
}

public abstract record ValidationError(string PropertyName, string ErrorMessage);