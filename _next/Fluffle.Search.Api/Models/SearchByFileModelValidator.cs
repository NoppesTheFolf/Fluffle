using FluentValidation;

namespace Fluffle.Search.Api.Models;

public class SearchByFileModelValidator : AbstractValidator<SearchByFileModel>
{
    private const int MaximumFileSize = 4 * 1024 * 1024;

    public SearchByFileModelValidator()
    {
        RuleFor(x => x.File)
            .NotEmpty()
            .Must(x => x == null || x.Length <= MaximumFileSize)
            .WithMessage($"The submitted file is larger than the maximum allowed size, which is {MaximumFileSize} bytes (4 MiB).");

        RuleFor(x => x.Limit).InclusiveBetween(8, 32);
    }
}
