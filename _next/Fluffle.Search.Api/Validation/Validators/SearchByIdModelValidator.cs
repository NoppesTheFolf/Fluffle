using FluentValidation;
using Fluffle.Search.Api.Models;

namespace Fluffle.Search.Api.Validation.Validators;

public class SearchByIdModelValidator : AbstractValidator<SearchByIdModel>
{
    public SearchByIdModelValidator()
    {
        RuleFor(x => x.Id).ValidateAsId();
        RuleFor(x => x.Limit).ValidateAsLimit();
    }
}
