using FluentValidation;

namespace Fluffle.Search.Api.Models;

public class SearchByIdModelValidator : AbstractValidator<SearchByIdModel>
{
    public SearchByIdModelValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(32)
            .Must(x => x == null || x.All(char.IsAsciiLetterOrDigit))
            .WithMessage("'Id' contains invalid characters.");

        RuleFor(x => x.Limit).InclusiveBetween(8, 32);
    }
}
