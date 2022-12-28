using FluentValidation;
using System.Collections.Generic;

namespace Noppes.Fluffle.Main.Communication
{
    public class SearchContentModel
    {
        public ICollection<string> References { get; set; }
    }

    public class SearchContentModelValidator : AbstractValidator<SearchContentModel>
    {
        public SearchContentModelValidator()
        {
            RuleFor(x => x.References).NotEmpty();
        }
    }
}
