using Newtonsoft.Json;

namespace Noppes.Fluffle.DeviantArt.Client.Models;

public class DeviationFile
{
    [JsonProperty("src")]
    public string Location { get; set; } = null!;
}

public interface IDeviationFileResolution
{
    public int Width { get; set; }

    public int Height { get; set; }
}

public class DeviationImageFile : DeviationFile, IDeviationFileResolution
{
    public int Width { get; set; }

    public int Height { get; set; }

    public bool Transparency { get; set; }

    [JsonProperty("filesize")]
    public int? FileSize { get; set; }
}

public class DeviationVideoFile : DeviationFile
{
    public string Quality { get; set; } = null!;

    [JsonProperty("filesize")]
    public int FileSize { get; set; }

    public int Duration { get; set; }
}

public class DeviationFlashFile : DeviationFile, IDeviationFileResolution
{
    public int Width { get; set; }

    public int Height { get; set; }
}
