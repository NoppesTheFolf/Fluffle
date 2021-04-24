using MessagePack;
using Noppes.Fluffle.Constants;
using System.Collections.Generic;

namespace Noppes.Fluffle.Main.Communication
{
    [MessagePackObject]
    public class CreditableEntitiesSyncModel : ITrackableModel<CreditableEntitiesSyncModel.CreditableEntityModel>
    {
        [MessagePackObject]
        public class CreditableEntityModel
        {
            [Key(0)]
            public int Id { get; set; }

            [Key(1)]
            public int PlatformId { get; set; }

            [Key(2)]
            public long ChangeId { get; set; }

            [Key(3)]
            public CreditableEntityType Type { get; set; }

            [Key(4)]
            public string Name { get; set; }
        }

        [Key(0)]
        public long NextChangeId { get; set; }

        [Key(1)]
        public IEnumerable<CreditableEntityModel> Results { get; set; }
    }
}
