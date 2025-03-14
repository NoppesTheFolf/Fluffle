﻿using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database.Models;

namespace Noppes.Fluffle.Main.Api.Mappers;

public class PutContentModelMapper : IMapper<PutContentModel, Content>
{
    public void MapFrom(PutContentModel src, Content dest)
    {
        dest.Reference = src.Reference;
        dest.IdOnPlatform = src.IdOnPlatform;

        if (int.TryParse(src.IdOnPlatform, out var idOnPlatformAsInteger))
            dest.IdOnPlatformAsInteger = idOnPlatformAsInteger;

        dest.Title = src.Title;
        dest.ViewLocation = src.ViewLocation;
        dest.RatingId = (int)src.Rating;
        dest.MediaTypeId = (int)src.MediaType;
        dest.Priority = src.Priority;
        dest.RequiresIndexing = src.ShouldBeIndexed;
    }
}

public class PutImageModelMapper : IMapper<PutContentModel, Image>
{
    public void MapFrom(PutContentModel src, Image dest)
    {
        src.MapTo((Content)dest);
    }
}
