using MessagePack;

namespace Noppes.Fluffle.Main.Communication
{
    [MessagePackObject]
    public class FaPopularArtistModel
    {
        [Key(0)]
        public string Artist { get; set; }

        [Key(1)]
        public int Score { get; set; }
    }
}
