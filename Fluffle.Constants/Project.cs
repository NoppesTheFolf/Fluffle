using System.Diagnostics;

namespace Noppes.Fluffle.Constants
{
    public static class Project
    {
        /// <summary>
        /// Version of Fluffle.
        /// </summary>
        public static string Version => Debugger.IsAttached ? "development" : "0.10.1";

        /// <summary>
        /// Base name of the User Agent used by Fluffle its synchronization and indexing bots.
        /// </summary>
        public static string UserAgentBase = "fluffle-bot";

        /// <summary>
        /// My username.
        /// </summary>
        public static readonly string DeveloperUsername = "Noppes";

        /// <summary>
        /// Where I can be contacted.
        /// </summary>
        public static readonly string DeveloperUrl = "fluffle.xyz/contact";

        /// <summary>
        /// User Agent used by Fluffle its synchronization and indexing bots.
        /// </summary>
        public static string UserAgent => $"{UserAgentBase}/{Version} (by {DeveloperUsername} at {DeveloperUrl})";
    }
}
