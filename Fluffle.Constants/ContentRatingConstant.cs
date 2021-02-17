namespace Noppes.Fluffle.Constants
{
    /// <summary>
    /// Content ratings recognized by Fluffle.
    /// </summary>
    public enum ContentRatingConstant
    {
        Safe = 1,
        Questionable = 2,
        Explicit = 3
    }

    public static class ContentRatingExtensions
    {
        /// <summary>
        /// Whether a rating should be considered Safe For Work (SFW) or not.
        /// </summary>
        public static bool IsSfw(this ContentRatingConstant rating) => rating == ContentRatingConstant.Safe;
    }
}
