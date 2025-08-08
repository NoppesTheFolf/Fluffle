using FluentValidation;
using Fluffle.Ingestion.Api.Models.ItemActions;

namespace Fluffle.Ingestion.Api.Validation;

public class PutDeleteGroupItemActionModelValidator : AbstractValidator<PutDeleteGroupItemActionModel>
{
    public PutDeleteGroupItemActionModelValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty();
    }
}
