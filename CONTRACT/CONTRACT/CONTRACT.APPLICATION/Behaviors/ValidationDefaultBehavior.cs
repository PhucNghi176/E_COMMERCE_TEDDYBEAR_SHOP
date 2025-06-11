using FluentValidation;
using MediatR;

namespace CONTRACT.CONTRACT.APPLICATION.Behaviors;

public sealed class ValidationDefaultBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator> _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var errorsDictionary = _validators
        .Select(x => x.Validate(context))
        .SelectMany(x => x.Errors)
        .Where(x => x != null)
        .GroupBy(x => new { x.PropertyName, x.ErrorMessage })
        .Select(x => x.FirstOrDefault())
        .ToList();

        if (errorsDictionary.Count != 0)
            throw new ValidationException(errorsDictionary);

        return await next();
    }
}