namespace Fluffle.Feeder.Legacy.MainApi;

public class ImagesSyncModel
{
    public long NextChangeId { get; set; }

    public ICollection<ImageModel> Results { get; set; } = null!;
}
