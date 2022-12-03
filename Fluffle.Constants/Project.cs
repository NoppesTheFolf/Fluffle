using System.Diagnostics;

namespace Noppes.Fluffle.Constants
{
    public static class Project
    {
        /// <summary>
        /// Version of Fluffle.
        /// </summary>
        public static string Version => Debugger.IsAttached ? "development" : "0.19.0";

        /// <summary>
        /// My username.
        /// </summary>
        public static readonly string DeveloperUsername = "Noppes";

        /// <summary>
        /// Where I can be contacted.
        /// </summary>
        public static readonly string DeveloperUrl = "fluffle.xyz/contact";

        /// <summary>
        /// Base name of the User Agent used by Fluffle's applications.
        /// </summary>
        public static string UserAgentBase(string applicationName) => $"fluffle-{applicationName}";

        /// <summary>
        /// User Agent used by Fluffle's applications.
        /// </summary>
        public static string UserAgent(string applicationName) => $"{UserAgentBase(applicationName)}/{Version} (by {DeveloperUsername} at {DeveloperUrl})";
    }
}
