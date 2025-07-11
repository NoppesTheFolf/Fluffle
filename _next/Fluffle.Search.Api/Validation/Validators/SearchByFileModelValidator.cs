using FluentValidation;
using Fluffle.Search.Api.Models;

namespace Fluffle.Search.Api.Validation.Validators;

public class SearchByFileModelValidator : AbstractValidator<SearchByFileModel>
{
    public SearchByFileModelValidator()
    {
        RuleFor(x => x.File).ValidateAsFile();
        RuleFor(x => x.Limit).ValidateAsLimit();
    }
}
