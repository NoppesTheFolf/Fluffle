namespace Noppes.Fluffle.DeviantArt.Client;

public class DeviantArtResponse<T, TError>
{
    public T? Value { get; set; }

    public TError? Error { get; set; }

    public DeviantArtResponse(T value)
    {
        Value = value;
    }

    public DeviantArtResponse(TError? error)
    {
        Error = error;
    }
}
