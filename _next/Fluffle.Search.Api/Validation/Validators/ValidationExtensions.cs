using FluentValidation;

namespace Fluffle.Search.Api.Validation.Validators;

public static class ValidationExtensions
{
    private const int MaximumFileSize = 4 * 1024 * 1024;

    public static IRuleBuilder<T, string> ValidateAsId<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(32)
            .Must(x => x == null || x.All(char.IsAsciiLetterOrDigit))
            .WithMessage("'Id' contains invalid characters.");
    }

    public static IRuleBuilder<T, int> ValidateAsLimit<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(8, 32);
    }

    public static IRuleBuilder<T, IFormFile> ValidateAsFile<T>(this IRuleBuilder<T, IFormFile> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .Must(x => x == null || x.Length <= MaximumFileSize)
            .WithMessage("The submitted file is larger than the maximum allowed size, which is 4 MiB.");
    }
}
