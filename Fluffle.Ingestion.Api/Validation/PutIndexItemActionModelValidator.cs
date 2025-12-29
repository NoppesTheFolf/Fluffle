using FluentValidation;
using Fluffle.Ingestion.Api.Models.ItemActions;
using System.Text.Json;

namespace Fluffle.Ingestion.Api.Validation;

public class PutIndexItemActionModelValidator : AbstractValidator<PutIndexItemActionModel>
{
    public PutIndexItemActionModelValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();

        RuleFor(x => x)
            .Must(x => (x.GroupId == null && x.GroupItemIds == null) || (x.GroupId != null && x.GroupItemIds != null))
            .WithMessage("'GroupId' and 'GroupItemIds' must both be set when either is provided.")
            .Must(x => x.GroupItemIds == null || x.GroupItemIds.Contains(x.ItemId))
            .WithMessage("'GroupItemIds' must at least contain 'ItemId'.");

        RuleForEach(x => x.GroupItemIds)
            .NotEmpty();

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
