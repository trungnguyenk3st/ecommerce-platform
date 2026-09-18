using FluentValidation.Results;

namespace ECommerce.Application.Common.Exceptions;

/// <summary>
/// Field-level validation failure raised directly by application services (e.g. Identity errors).
/// Named "App..." to avoid colliding with FluentValidation's own ValidationException, which
/// ValidateAndThrowAsync() throws and which the API's exception middleware also maps to 400.
/// </summary>
public class AppValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public AppValidationException() : base("One or more validation failures occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public AppValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = ToErrorDictionary(failures);
    }

    public static IDictionary<string, string[]> ToErrorDictionary(IEnumerable<ValidationFailure> failures) =>
        failures.GroupBy(f => f.PropertyName, f => f.ErrorMessage).ToDictionary(g => g.Key, g => g.ToArray());
}

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message = "You do not have permission to perform this action.") : base(message) { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Invalid credentials.") : base(message) { }
}
