using FluentValidation;
using Fluffle.Ingestion.Api.Models.ItemActions;

namespace Fluffle.Ingestion.Api.Validation;

public class PutItemActionModelValidator : AbstractValidator<PutItemActionModel>
{
    public PutItemActionModelValidator()
    {
        RuleFor(x => x).SetInheritanceValidator(x =>
        {
            x.Add(new PutIndexItemActionModelValidator());
            x.Add(new PutDeleteItemActionModelValidator());
            x.Add(new PutDeleteGroupItemActionModelValidator());
        });
    }
}
