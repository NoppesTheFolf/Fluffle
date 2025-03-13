namespace Noppes.Fluffle.FurAffinity.Models;

public class FaResult<T>
{
    public FaOnlineStats Stats { get; set; }

    public T Result { get; set; }
}
