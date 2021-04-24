using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database.Models;

namespace Noppes.Fluffle.Main.Api.Mappers.FurAffinity
{
    public class PopularArtistMapper : IMapper<FaPopularArtist, FaPopularArtistModel>
    {
        public void MapFrom(FaPopularArtist src, FaPopularArtistModel dest)
        {
            dest.Artist = src.ArtistId;
            dest.Score = src.AverageScore;
        }
    }
}
