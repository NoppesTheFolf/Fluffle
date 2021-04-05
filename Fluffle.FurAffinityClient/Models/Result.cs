namespace Noppes.Fluffle.FurAffinity
{
    public class FaResult<T>
    {
        public FaOnlineStats Stats { get; set; }

        public T Result { get; set; }
    }

    public class FaOnlineStats
    {
        public int Online { get; set; }

        public int Guests { get; set; }

        public int Registered { get; set; }

        public int Other { get; set; }
    }
}
