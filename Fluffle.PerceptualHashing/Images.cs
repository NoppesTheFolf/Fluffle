namespace Noppes.Fluffle.PerceptualHashing;

public readonly struct HashedImage
{
    public int Id { get; }

    public ulong Hash { get; }

    public HashedImage(int id, ulong hash)
    {
        Id = id;
        Hash = hash;
    }

    public bool Equals(HashedImage other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        return obj is HashedImage other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}

public readonly struct ComparedImage
{
    public int Id { get; }

    public ulong MismatchCount { get; }

    public ComparedImage(int id, ulong mismatchCount)
    {
        Id = id;
        MismatchCount = mismatchCount;
    }
}
