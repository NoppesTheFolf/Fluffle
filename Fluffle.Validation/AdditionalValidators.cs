using FluentValidation;
using System;

namespace Noppes.Fluffle.Validation
{
    /// <summary>
    /// More validators extending Fluent Validation its rule binders.
    /// </summary>
    public static class AdditionalValidators
    {
        /// <summary>
        /// Defines a hostname validator on the current rule builder. The validator will fail if the
        /// provided value is empty or isn't a valid hostname according to the <see
        /// cref="Uri.CheckHostName"/> method.
        /// </summary>
        public static IRuleBuilderOptions<T, string> Hostname<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty()
                .Must(value => Uri.CheckHostName(value) != UriHostNameType.Unknown)
                .WithMessage("The string must be a valid hostname.");
        }

        /// <summary>
        /// Defines a length validator on the current rule builder. The validator will fail if the
        /// provided array isn't of the specified length.
        /// </summary>
        public static IRuleBuilderOptions<T, TProperty[]> Length<T, TProperty>(this IRuleBuilder<T, TProperty[]> ruleBuilder, int length)
        {
            return ruleBuilder
                .NotEmpty()
                .Must(value => value != null && value.Length == length)
                .WithMessage($"The array must contain exactly {length} elements.");
        }

        public static IRuleBuilder<T, string> IsWellFormedHttpUrl<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty()
                .Must(value => value == null || Uri.IsWellFormedUriString(value, UriKind.Absolute) && new Uri(value, UriKind.Absolute).Scheme is "http" or "https")
                .WithMessage("The string must be a well formatted HTTP(S) URL.");
        }
    }
}
