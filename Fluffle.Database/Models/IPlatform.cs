namespace Noppes.Fluffle.Database.Models
{
    public interface IPlatform
    {
        public int Id { get; set; }

        public string NormalizedName { get; set; }
    }
}
