using Humanizer;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Main.Database.Models;
using System;

namespace Noppes.Fluffle.Main.Api.Helpers
{
    public class DataSources : IDataSource<PlatformConstant, Platform>, IDataSource<FileFormatConstant, FileFormat>,
        IDataSource<MediaTypeConstant, MediaType>, IDataSource<ContentRatingConstant, ContentRating>, IDataSource<SyncTypeConstant, SyncType>
    {
        public Platform From(PlatformConstant value)
        {
            var platform = value switch
            {
                PlatformConstant.E621 => new Platform
                {
                    Name = "e621",
                    EstimatedContentCount = 2100000,
                    HomeLocation = "https://e621.net"
                },
                PlatformConstant.FurryNetwork => new Platform
                {
                    Name = "Furry Network",
                    EstimatedContentCount = 800_000,
                    HomeLocation = "https://furrynetwork.com"
                },
                PlatformConstant.FurAffinity => new Platform
                {
                    Name = "Fur Affinity",
                    EstimatedContentCount = 30_000_000,
                    HomeLocation = "https://www.furaffinity.net"
                },
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };

            platform.NormalizedName = platform.Name.Kebaberize();

            return platform;
        }

        public MediaType From(MediaTypeConstant value)
        {
            return value switch
            {
                MediaTypeConstant.Image => new MediaType
                {
                    Name = "Image"
                },
                MediaTypeConstant.AnimatedImage => new MediaType
                {
                    Name = "Animated image"
                },
                MediaTypeConstant.Video => new MediaType
                {
                    Name = "Video"
                },
                MediaTypeConstant.Other => new MediaType
                {
                    Name = "Other"
                },
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        public FileFormat From(FileFormatConstant value)
        {
            return value switch
            {
                FileFormatConstant.Png => new FileFormat
                {
                    Name = "Portable Network Graphics",
                    Abbreviation = "PNG",
                    Extension = "png"
                },
                FileFormatConstant.Jpeg => new FileFormat
                {
                    Name = "Joint Photographic Experts Group",
                    Abbreviation = "JPEG",
                    Extension = "jpg"
                },
                FileFormatConstant.WebM => new FileFormat
                {
                    Name = "WebM",
                    Abbreviation = "WebM",
                    Extension = "webm"
                },
                FileFormatConstant.Swf => new FileFormat
                {
                    Name = "Shockwave Flash",
                    Abbreviation = "SWF",
                    Extension = "swf"
                },
                FileFormatConstant.Gif => new FileFormat
                {
                    Name = "Graphics Interchange Format",
                    Abbreviation = "GIF",
                    Extension = "gif"
                },
                FileFormatConstant.WebP => new FileFormat
                {
                    Name = "WebP",
                    Abbreviation = "WebP",
                    Extension = "webp"
                },
                FileFormatConstant.Html => new FileFormat
                {
                    Name = "Hypertext Markup Language",
                    Abbreviation = "HTML",
                    Extension = "html"
                },
                FileFormatConstant.Pdf => new FileFormat
                {
                    Name = "Portable Document Format",
                    Abbreviation = "PDF",
                    Extension = "pdf"
                },
                FileFormatConstant.Rtf => new FileFormat
                {
                    Name = "Rich Text Format",
                    Abbreviation = "RTF",
                    Extension = "rtf"
                },
                FileFormatConstant.Txt => new FileFormat
                {
                    Name = "Text",
                    Abbreviation = "Text",
                    Extension = "txt"
                },
                FileFormatConstant.Doc => new FileFormat
                {
                    Name = "Document",
                    Abbreviation = "DOC",
                    Extension = "doc"
                },
                FileFormatConstant.Docx => new FileFormat
                {
                    Name = "Office Open XML",
                    Abbreviation = "DOCX",
                    Extension = "docx"
                },
                FileFormatConstant.Odt => new FileFormat
                {
                    Name = "OpenDocument Text",
                    Abbreviation = "ODT",
                    Extension = "odt"
                },
                FileFormatConstant.Mp3 => new FileFormat
                {
                    Name = "MPEG-1 Audio Layer III",
                    Abbreviation = "MP3",
                    Extension = "mp3"
                },
                FileFormatConstant.Wav => new FileFormat
                {
                    Name = "Waveform Audio File Format",
                    Abbreviation = "WAV",
                    Extension = ".wav"
                },
                FileFormatConstant.Mid => new FileFormat
                {
                    Name = "Standard MIDI",
                    Abbreviation = "MID",
                    Extension = ".mid"
                },
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        public ContentRating From(ContentRatingConstant value)
        {
            return value switch
            {
                ContentRatingConstant.Safe => new ContentRating
                {
                    Name = "Safe",
                    IsSfw = true
                },
                ContentRatingConstant.Questionable => new ContentRating
                {
                    Name = "Questionable",
                    IsSfw = false
                },
                ContentRatingConstant.Explicit => new ContentRating
                {
                    Name = "Explicit",
                    IsSfw = false
                },
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        public SyncType From(SyncTypeConstant value)
        {
            return value switch
            {
                SyncTypeConstant.Full => new SyncType
                {
                    Name = "Full sync"
                },
                SyncTypeConstant.Quick => new SyncType
                {
                    Name = "Quick sync"
                },
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }
    }
}
