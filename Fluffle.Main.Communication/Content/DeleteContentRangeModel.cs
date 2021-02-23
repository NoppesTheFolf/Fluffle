using FluentValidation;
using System.Collections.Generic;

namespace Noppes.Fluffle.Main.Communication
{
    public class DeleteContentRangeModel
    {
        public int ExclusiveStart { get; set; }

        public int InclusiveEnd { get; set; }

        public ICollection<int> ExcludedIds { get; set; }
    }

    public class DeleteContentRangeModelValidator : AbstractValidator<DeleteContentRangeModel>
    {
        public DeleteContentRangeModelValidator()
        {
            RuleFor(o => o.ExclusiveStart).GreaterThanOrEqualTo(0);
            RuleFor(o => o.ExcludedIds).NotNull();
        }
    }
}
