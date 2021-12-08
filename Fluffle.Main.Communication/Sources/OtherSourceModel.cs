using MessagePack;

namespace Noppes.Fluffle.Main.Communication
{
    [MessagePackObject]
    public class OtherSourceModel
    {
        [Key(0)]
        public int Id { get; set; }

        [Key(1)]
        public string Location { get; set; }
    }
}
