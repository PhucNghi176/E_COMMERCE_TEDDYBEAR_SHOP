using CONTRACT.CONTRACT.DOMAIN.Exceptions;

namespace QUERY.APPLICATION.Exceptions;

public sealed class ValidationException(IReadOnlyCollection<ValidationError> errors) : DomainException("Validation Failure", "One or more validation errors occurred")
{
    public IReadOnlyCollection<ValidationError> Errors { get; } = errors;
}

public record ValidationError(string PropertyName, string ErrorMessage);
