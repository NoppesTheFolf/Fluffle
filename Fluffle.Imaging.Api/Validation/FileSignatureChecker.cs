namespace Fluffle.Imaging.Api.Validation;

public class FileSignatureChecker
{
    private readonly List<byte?[]> _signatures = [];

    public void Add(byte?[] signature)
    {
        if (signature.Length == 0)
        {
            throw new ArgumentException("Signature may not be empty.", nameof(signature));
        }

        _signatures.Add(signature);
    }

    public async Task<bool> CheckAsync(Stream stream)
    {
        if (!stream.CanSeek)
        {
            throw new ArgumentException("Stream must be seekable.", nameof(stream));
        }

        if (_signatures.Count == 0)
        {
            return false;
        }

        var maximumSignatureLength = _signatures.Max(x => x.Length);
        var bufferSize = Math.Min(maximumSignatureLength, stream.Length - stream.Position);
        var buffer = new byte[bufferSize];

        var streamOriginalPosition = stream.Position;
        await stream.ReadExactlyAsync(buffer);
        stream.Position = streamOriginalPosition;

        foreach (var signature in _signatures)
        {
            if (signature.Length > buffer.Length)
            {
                continue;
            }

            var isMatch = true;
            for (var i = 0; i < signature.Length; i++)
            {
                if (signature[i] == null)
                {
                    continue;
                }

                if (signature[i] == buffer[i])
                {
                    continue;
                }

                isMatch = false;
                break;
            }

            if (isMatch)
            {
                return true;
            }
        }

        return false;
    }
}
