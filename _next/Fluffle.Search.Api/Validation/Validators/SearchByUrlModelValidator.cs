using FluentValidation;
using Fluffle.Search.Api.Models;

namespace Fluffle.Search.Api.Validation.Validators;

public class SearchByUrlModelValidator : AbstractValidator<SearchByUrlModel>
{
    public SearchByUrlModelValidator()
    {
        RuleFor(x => x.Url).NotEmpty();
        RuleFor(x => x.Limit).ValidateAsLimit();
    }
}
