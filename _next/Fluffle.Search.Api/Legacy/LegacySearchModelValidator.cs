using FluentValidation;

namespace Fluffle.Search.Api.Legacy;

public class LegacySearchModelValidator : AbstractValidator<LegacySearchModel>
{
    public LegacySearchModelValidator()
    {
        RuleFor(o => o.File)
            .NotNull()
            .WithMessage("You forgot to provide an image. Make sure you add the 'file' field to your request as a file.");

        RuleFor(o => o.Limit)
            .InclusiveBetween(8, 32);
    }
}
