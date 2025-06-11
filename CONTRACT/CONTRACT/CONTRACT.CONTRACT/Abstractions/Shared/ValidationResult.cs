namespace CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
public class ValidationResult : Result, IValidationResult
{
    protected internal ValidationResult(Error[] errors) : base(false, IValidationResult.ValidationError)
    {
        Errors = errors;
    }

    public Error[] Errors { get; }

    public static ValidationResult WithErrors(Error[] errors)
    {
        return new ValidationResult(errors);
    }
}