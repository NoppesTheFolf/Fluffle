using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Noppes.Fluffle.Search.Api.Models
{
    public class SearchModel
    {
        public static readonly int DefaultLimit = 32;

        public int Limit { get; set; } = DefaultLimit;

        public bool IncludeNsfw { get; set; } = false;

        public IFormFile Image { get; set; }
    }

    public class SearchModelValidator : AbstractValidator<SearchModel>
    {
        public static readonly int MinimumLimit = 8;
        public static readonly int MaximumLimit = 128;

        public SearchModelValidator()
        {
            RuleFor(o => o.Limit).InclusiveBetween(MinimumLimit, MaximumLimit);

            RuleFor(o => o.Image).NotNull();
        }
    }
}
