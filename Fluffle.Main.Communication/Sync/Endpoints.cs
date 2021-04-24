namespace Noppes.Fluffle.Main.Communication
{
    public static partial class Endpoints
    {
        public const string Sync = "sync";

        public static object[] GetSyncImages(string platformName, long afterChangeId) =>
            V1.Url(Sync, "images", platformName, afterChangeId);

        public static object[] GetSyncCreditableEntities(string platformName, long afterChangeId) =>
            V1.Url(Sync, "creditable-entities", platformName, afterChangeId);
    }
}
