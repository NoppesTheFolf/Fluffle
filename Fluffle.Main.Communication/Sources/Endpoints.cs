namespace Noppes.Fluffle.Main.Communication
{
    public static partial class Endpoints
    {
        public const string Source = "source";
        public const string Sources = "sources";
        public const int SourcesLimit = 2500;

        public static object[] GetOtherSources(int afterId) =>
            V1.Url(Sources, "other", afterId);
    }
}
