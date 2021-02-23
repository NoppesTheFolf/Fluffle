namespace Noppes.Fluffle.Main.Communication
{
    public static partial class Endpoints
    {
        public const string Status = "status";

        public static object[] GetStatus =>
            V1.Url(Status);
    }
}
