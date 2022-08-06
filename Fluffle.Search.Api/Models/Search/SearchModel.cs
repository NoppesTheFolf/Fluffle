using FluentValidation;
using FluentValidation.Results;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Noppes.Fluffle.Constants;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Noppes.Fluffle.Search.Api.Models
{
    public class SearchModel
    {
        public IFormFile File { get; set; }

        public bool IncludeNsfw { get; set; }

        [ModelBinder(Name = nameof(Platforms))]
        public IEnumerable<string> PlatformNames { get; set; }

        [BindNever]
        public ImmutableHashSet<PlatformConstant> Platforms { get; set; }

        public int Limit { get; set; } = 32;

        public bool CreateLink { get; set; } = false;
    }

    public class SearchModelValidator : AbstractValidator<SearchModel>
    {
        private static readonly IDictionary<string, PlatformConstant> LookupDictionary =
            Enum.GetValues<PlatformConstant>().ToDictionary(Enum.GetName);

        private static readonly ImmutableHashSet<PlatformConstant> AllPlatforms =
            LookupDictionary.Values.ToImmutableHashSet();

        public static readonly int MinimumLimit = 8;
        public static readonly int MaximumLimit = 32;
        public static readonly int AreaMax = 4000 * 4000;
        public static readonly int SizeMax = 4 * 1024 * 1024;

        public SearchModelValidator()
        {
            RuleFor(o => o.IncludeNsfw);
            RuleFor(o => o.Limit).InclusiveBetween(MinimumLimit, MaximumLimit);

            RuleFor(o => o.File)
                .NotNull()
                .WithMessage("You forgot to provide an image. Make sure you add the 'file' field to your request as a file.");

            RuleFor(o => o.CreateLink);
        }

        protected override bool PreValidate(ValidationContext<SearchModel> context, ValidationResult result)
        {
            var model = context.InstanceToValidate;

            if (model.PlatformNames == null)
            {
                model.Platforms = AllPlatforms;
                return true;
            }

            var platforms = new List<PlatformConstant>();
            var platformNames = model.PlatformNames
                .Select(s => (s.Trim(), s.Trim().Pascalize()));

            foreach (var (name, normalizedName) in platformNames)
            {
                if (LookupDictionary.TryGetValue(normalizedName, out var platform))
                {
                    platforms.Add(platform);
                }
                else
                {
                    result.Errors.Add(new ValidationFailure(nameof(SearchModel.Platforms), $"Platform with the name '{name}' either doesn't exist or is not supported."));
                    return false;
                }
            }

            model.Platforms = platforms.ToImmutableHashSet();
            return true;
        }
    }
}
