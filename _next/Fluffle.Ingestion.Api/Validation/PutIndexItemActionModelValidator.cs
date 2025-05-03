using FluentValidation;
using Fluffle.Ingestion.Api.Models.ItemActions;
using System.Text.Json;

namespace Fluffle.Ingestion.Api.Validation;

public class PutIndexItemActionModelValidator : AbstractValidator<PutIndexItemActionModel>
{
    public PutIndexItemActionModelValidator()
    {
        RuleForEach(x => x.Images)
            .NotEmpty()
            .ChildRules(image =>
            {
                image.RuleFor(x => x.Width).GreaterThan(0);
                image.RuleFor(x => x.Height).GreaterThan(0);
                image.RuleFor(x => x.Url).NotEmpty();
            });

        RuleFor(x => x.Properties)
            .NotNull()
            .Must(x => x == null || x.GetValueKind() == JsonValueKind.Object)
            .WithMessage("'{PropertyName}' should be an object.");
    }
}
