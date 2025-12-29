using FluentValidation;
using Fluffle.Search.Api.Models;

namespace Fluffle.Search.Api.Validation.Validators;

public class CreateLinkModelValidator : AbstractValidator<CreateLinkModel>
{
    public CreateLinkModelValidator()
    {
        RuleFor(x => x.File).ValidateAsFile();
    }
}
