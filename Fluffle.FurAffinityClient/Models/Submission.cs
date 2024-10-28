using System;
using System.Collections.Generic;

namespace Noppes.Fluffle.FurAffinity;

public enum FaSubmissionCategory
{
    All,
    ArtworkDigital,
    ArtworkTraditional,
    CelShading,
    Crafting,
    Designs,
    Flash,
    Fursuiting,
    Icons,
    Mosaics,
    Photography,
    FoodRecipes,
    Sculpting,
    Story,
    Poetry,
    Prose,
    Music,
    Podcasts,
    Skins,
    Handhelds,
    Resources,
    Adoptables,
    Other,
    Auctions,
    Contests,
    CurrentEvents,
    Desktops,
    Stockart,
    Screenshots,
    Scraps,
    Wallpaper,
    YchSale
}

public static class SubmissionCategoryHelper
{
    private static readonly IReadOnlyDictionary<string, FaSubmissionCategory> Mappings = new Dictionary<string, FaSubmissionCategory>
    {
        { "All", FaSubmissionCategory.All },
        { "Artwork (Digital)", FaSubmissionCategory.ArtworkDigital },
        { "Artwork (Traditional)", FaSubmissionCategory.ArtworkTraditional },
        { "Cel Shading", FaSubmissionCategory.CelShading },
        { "Crafting", FaSubmissionCategory.Crafting },
        { "Designs", FaSubmissionCategory.Designs },
        { "Flash", FaSubmissionCategory.Flash },
        { "Fursuiting", FaSubmissionCategory.Fursuiting },
        { "Icons", FaSubmissionCategory.Icons },
        { "Mosaics", FaSubmissionCategory.Mosaics },
        { "Photography", FaSubmissionCategory.Photography },
        { "Food / Recipes", FaSubmissionCategory.FoodRecipes },
        { "Sculpting", FaSubmissionCategory.Sculpting },
        { "Story", FaSubmissionCategory.Story },
        { "Poetry", FaSubmissionCategory.Poetry },
        { "Prose", FaSubmissionCategory.Prose },
        { "Music", FaSubmissionCategory.Music },
        { "Podcasts", FaSubmissionCategory.Podcasts },
        { "Skins", FaSubmissionCategory.Skins },
        { "Handhelds", FaSubmissionCategory.Handhelds },
        { "Resources", FaSubmissionCategory.Resources },
        { "Adoptables", FaSubmissionCategory.Adoptables },
        { "Other", FaSubmissionCategory.Other },
        { "Auctions", FaSubmissionCategory.Auctions },
        { "Contests", FaSubmissionCategory.Contests },
        { "Current Events", FaSubmissionCategory.CurrentEvents },
        { "Desktops", FaSubmissionCategory.Desktops },
        { "Stockart", FaSubmissionCategory.Stockart },
        { "Screenshots", FaSubmissionCategory.Screenshots },
        { "Scraps", FaSubmissionCategory.Scraps },
        { "Wallpaper", FaSubmissionCategory.Wallpaper },
        { "YCH / Sale", FaSubmissionCategory.YchSale }
    };

    public static FaSubmissionCategory CategoryFromString(string category)
    {
        foreach (var mapping in Mappings)
            if (category.StartsWith(mapping.Key))
                return mapping.Value;

        throw new ArgumentException(null, nameof(category));
    }
}

public enum FaSubmissionType
{
    All,
    Abstract,
    AnimalRelatedNonAnthro,
    Anime,
    Comics,
    Doodle,
    Fanart,
    Fantasy,
    Human,
    Portraits,
    Scenery,
    StillLife,
    Tutorials,
    Miscellaneous,
    Babyfur,
    Bondage,
    Digimon,
    FatFurs,
    FetishOther,
    Fursuit,
    GoreMacabreArt,
    Hyper,
    Hypnosis,
    Inflation,
    MacroMicro,
    Muscle,
    MyLittlePonyBrony,
    Paw,
    Pokemon,
    Pregnancy,
    Sonic,
    Transformation,
    TFTG,
    Vore,
    WaterSports,
    GeneralFurryArt,
    Techno,
    Trance,
    House,
    Y90s,
    Y80s,
    Y70s,
    Y60s,
    PreY60s,
    Classical,
    GameMusic,
    Rock,
    Pop,
    Rap,
    Industrial,
    OtherMusic
}

public static class SubmissionTypeHelper
{
    private static readonly IReadOnlyDictionary<string, FaSubmissionType> _mappings = new Dictionary<string, FaSubmissionType>
    {
        {"All", FaSubmissionType.All},
        {"Abstract", FaSubmissionType.Abstract},
        {"Animal related (non-anthro)", FaSubmissionType.AnimalRelatedNonAnthro},
        {"Anime", FaSubmissionType.Anime},
        {"Comics", FaSubmissionType.Comics},
        {"Doodle", FaSubmissionType.Doodle},
        {"Fanart", FaSubmissionType.Fanart},
        {"Fantasy", FaSubmissionType.Fantasy},
        {"Human", FaSubmissionType.Human},
        {"Portraits", FaSubmissionType.Portraits},
        {"Scenery", FaSubmissionType.Scenery},
        {"Still Life", FaSubmissionType.StillLife},
        {"Tutorials", FaSubmissionType.Tutorials},
        {"Miscellaneous", FaSubmissionType.Miscellaneous},
        {"Baby fur", FaSubmissionType.Babyfur},
        {"Bondage", FaSubmissionType.Bondage},
        {"Digimon", FaSubmissionType.Digimon},
        {"Fat Furs", FaSubmissionType.FatFurs},
        {"Fetish Other", FaSubmissionType.FetishOther},
        {"Fursuit", FaSubmissionType.Fursuit},
        {"Gore / Macabre Art", FaSubmissionType.GoreMacabreArt},
        {"Hyper", FaSubmissionType.Hyper},
        {"Hypnosis", FaSubmissionType.Hypnosis},
        {"Inflation", FaSubmissionType.Inflation},
        {"Macro / Micro", FaSubmissionType.MacroMicro},
        {"Muscle", FaSubmissionType.Muscle},
        {"My Little Pony / Brony", FaSubmissionType.MyLittlePonyBrony},
        {"Paw", FaSubmissionType.Paw},
        {"Pokemon", FaSubmissionType.Pokemon},
        {"Pregnancy", FaSubmissionType.Pregnancy},
        {"Sonic", FaSubmissionType.Sonic},
        {"Transformation", FaSubmissionType.Transformation},
        {"TF / TG", FaSubmissionType.TFTG},
        {"Vore", FaSubmissionType.Vore},
        {"Water Sports", FaSubmissionType.WaterSports},
        {"General Furry Art", FaSubmissionType.GeneralFurryArt},
        {"Techno", FaSubmissionType.Techno},
        {"Trance", FaSubmissionType.Trance},
        {"House", FaSubmissionType.House},
        {"90s", FaSubmissionType.Y90s},
        {"80s", FaSubmissionType.Y80s},
        {"70s", FaSubmissionType.Y70s},
        {"60s", FaSubmissionType.Y60s},
        {"Pre-60s", FaSubmissionType.PreY60s},
        {"Classical", FaSubmissionType.Classical},
        {"Game Music", FaSubmissionType.GameMusic},
        {"Rock", FaSubmissionType.Rock},
        {"Pop", FaSubmissionType.Pop},
        {"Rap", FaSubmissionType.Rap},
        {"Industrial", FaSubmissionType.Industrial},
        {"Other Music", FaSubmissionType.OtherMusic}
    };

    public static FaSubmissionType TypeFromString(string category)
    {
        foreach (var mapping in _mappings)
            if (category.EndsWith(mapping.Key))
                return mapping.Value;

        throw new ArgumentException(null, nameof(category));
    }
}

public class FaSubmission
{
    public int Id { get; set; }

    public FaArtist Owner { get; set; }

    public string Title { get; set; }

    public FaSubmissionStats Stats { get; set; }

    public FaSubmissionRating Rating { get; set; }

    public FaSubmissionCategory? Category { get; set; }

    public FaSubmissionType? Type { get; set; }

    public string Species { get; set; }

    public string Gender { get; set; }

    public Uri ViewLocation { get; set; }

    public Uri FileLocation { get; set; }

    public FaSize Size { get; set; }

    public DateTimeOffset ThumbnailWhen { get; set; }

    public DateTimeOffset When { get; set; }

    public FaThumbnail GetThumbnail(int targetMax)
    {
        var thumbnail = new FaThumbnail
        {
            Location = new Uri($"https://t.furaffinity.net/{Id}@{targetMax}-{ThumbnailWhen.ToUnixTimeSeconds()}.jpg")
        };

        if (Size == null)
        {
            (thumbnail.Width, thumbnail.Height) = (-1, -1);
            return thumbnail;
        }

        static int DetermineSize(int sizeOne, int sizeTwo, int sizeOneTarget)
        {
            var aspectRatio = (double)sizeOneTarget / sizeOne;

            return (int)Math.Round(aspectRatio * sizeTwo);
        }

        if (Size.Width == Size.Height)
        {
            (thumbnail.Width, thumbnail.Height) = (targetMax, targetMax);
            return thumbnail;
        }

        (thumbnail.Width, thumbnail.Height) = Size.Width > Size.Height
            ? (DetermineSize(Size.Width, Size.Height, targetMax), targetMax)
            : (targetMax, DetermineSize(Size.Height, Size.Width, targetMax));

        return thumbnail;
    }
}

public class FaSize
{
    public int Width { get; set; }

    public int Height { get; set; }

    public FaSize()
    {
    }

    public FaSize(int width, int height)
    {
        Width = width;
        Height = height;
    }
}

public class FaArtist
{
    public string Id { get; set; }

    public string Name { get; set; }
}

public class FaSubmissionStats
{
    public int Views { get; set; }

    public int Comments { get; set; }

    public int Favorites { get; set; }
}

public enum FaSubmissionRating
{
    General,
    Mature,
    Adult
}

public class FaThumbnail
{
    public Uri Location { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }
}

public class FaGallerySubmission
{
    public int Id { get; set; }

    public string ArtistId { get; set; }
}
