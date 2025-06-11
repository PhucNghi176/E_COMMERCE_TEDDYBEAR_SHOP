namespace CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;

public interface IValidationResult
{
    public static readonly Error ValidationError = new("Validation Error", "A validation error occured");
    Error[] Errors { get; }
}