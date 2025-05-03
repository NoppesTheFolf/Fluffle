using FluentValidation;
using Fluffle.Ingestion.Api.Models.ItemActions;

namespace Fluffle.Ingestion.Api.Validation;

public class PutDeleteItemActionModelValidator : AbstractValidator<PutDeleteItemActionModel>
{
    public PutDeleteItemActionModelValidator()
    {
    }
}
