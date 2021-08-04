using FluentValidation;
using FluentValidation.Results;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Noppes.Fluffle.Constants;
using System;
using System.Collections.Generic;
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
        public ICollection<PlatformConstant> Platforms { get; set; }

        public int Limit { get; set; } = 32;
    }

    public class SearchModelValidator : AbstractValidator<SearchModel>
    {
        private static readonly IDictionary<string, PlatformConstant> LookupDictionary =
            Enum.GetValues<PlatformConstant>().ToDictionary(Enum.GetName);

        public static readonly int MinimumLimit = 8;
        public static readonly int MaximumLimit = 128;
        public static readonly int AreaMax = 4000 * 4000;
        public static readonly int SizeMax = 4 * 1024 * 1024;

        public SearchModelValidator()
        {
            RuleFor(o => o.IncludeNsfw);
            RuleFor(o => o.Limit).InclusiveBetween(MinimumLimit, MaximumLimit);

            RuleFor(o => o.File)
                .NotNull()
                .WithMessage("You forgot to provide an image. Make sure you add the 'file' field to your request as a file.");
        }

        protected override bool PreValidate(ValidationContext<SearchModel> context, ValidationResult result)
        {
            var model = context.InstanceToValidate;

            if (model.File != null && model.File.Length > SizeMax)
            {
                result.Errors.Add(new ValidationFailure(nameof(SearchModel.File), $"The submitted file has a size of {model.File.Length} bytes while the maximum allowed size is {SizeMax} bytes (4 MiB)."));
                return false;
            }

            if (model.PlatformNames == null)
            {
                model.Platforms = LookupDictionary.Values;
                return true;
            }

            model.Platforms = new List<PlatformConstant>();
            var platformNames = model.PlatformNames
                .Select(s => (s.Trim(), s.Trim().Pascalize()));

            foreach (var (name, normalizedName) in platformNames)
            {
                if (LookupDictionary.TryGetValue(normalizedName, out var platform))
                {
                    model.Platforms.Add(platform);
                }
                else
                {
                    result.Errors.Add(new ValidationFailure(nameof(SearchModel.Platforms), $"Platform with the name '{name}' either doesn't exist or is not supported."));
                    return false;
                }
            }

            return true;
        }
    }
}
