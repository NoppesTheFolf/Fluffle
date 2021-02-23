using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database.Models;

namespace Noppes.Fluffle.Main.Api.Mappers
{
    public class PutImageIndexModelMapper : IMapper<PutImageIndexModel, ImageHash>
    {
        public void MapFrom(PutImageIndexModel src, ImageHash dest)
        {
            dest.PhashRed64 = src.Hashes.PhashRed64;
            dest.PhashGreen64 = src.Hashes.PhashGreen64;
            dest.PhashBlue64 = src.Hashes.PhashBlue64;
            dest.PhashAverage64 = src.Hashes.PhashAverage64;
            dest.PhashRed256 = src.Hashes.PhashRed256;
            dest.PhashGreen256 = src.Hashes.PhashGreen256;
            dest.PhashBlue256 = src.Hashes.PhashBlue256;
            dest.PhashAverage256 = src.Hashes.PhashAverage256;
            dest.PhashRed1024 = src.Hashes.PhashRed1024;
            dest.PhashGreen1024 = src.Hashes.PhashGreen1024;
            dest.PhashBlue1024 = src.Hashes.PhashBlue1024;
            dest.PhashAverage1024 = src.Hashes.PhashAverage1024;
        }
    }
}
