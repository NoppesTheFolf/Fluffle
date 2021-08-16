using System.Diagnostics;

namespace Noppes.Fluffle.Constants
{
    public static class Project
    {
        /// <summary>
        /// Version of Fluffle.
        /// </summary>
        public static string Version => Debugger.IsAttached ? "development" : "0.6.1";
    }
}
