namespace Noppes.Fluffle.Search.Domain;

public readonly struct ImageHashes
{
    public required ulong PhashAverage64 { get; init; }

    public required ulong[] PhashRed256 { get; init; }
    public required ulong[] PhashGreen256 { get; init; }
    public required ulong[] PhashBlue256 { get; init; }
    public required ulong[] PhashAverage256 { get; init; }

    public required ulong[] PhashRed1024 { get; init; }
    public required ulong[] PhashGreen1024 { get; init; }
    public required ulong[] PhashBlue1024 { get; init; }
    public required ulong[] PhashAverage1024 { get; init; }
}
