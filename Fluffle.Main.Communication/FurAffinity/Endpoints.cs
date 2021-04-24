namespace Noppes.Fluffle.Main.Communication
{
    public static partial class Endpoints
    {
        public const string FurAffinity = "fur-affinity";

        public static object[] GetFaBotsAllowed() =>
            V1.Url(FurAffinity, "bots-allowed");

        public static object[] GetFaPopularArtists() =>
            V1.Url(FurAffinity, "popular-artists");
    }
}
