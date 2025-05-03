using FluentValidation;
using Fluffle.Ingestion.Api.Controllers;
using Fluffle.Ingestion.Api.Models.ItemActions;

namespace Fluffle.Ingestion.Api.Validation;

public class PutItemActionModelCollectionValidator : AbstractValidator<ICollection<PutItemActionModel>>
{
    public PutItemActionModelCollectionValidator()
    {
        RuleForEach(x => x)
            .NotNull()
            .SetValidator(new PutItemActionModelValidator());
    }
}