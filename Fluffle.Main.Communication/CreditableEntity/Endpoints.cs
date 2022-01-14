namespace Noppes.Fluffle.Main.Communication
{
    public static partial class Endpoints
    {
        public const string CreditableEntity = "creditable-entity";

        public static object[] GetCreditablyEntityMaxPriority(string platformName, string creditableEntityName) =>
            V1.Url(PlatformRoute(platformName, CreditableEntity, creditableEntityName, "priorities", "max"));
    }
}
