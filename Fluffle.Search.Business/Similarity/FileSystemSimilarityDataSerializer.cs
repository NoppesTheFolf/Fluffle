using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Utils;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace Noppes.Fluffle.Search.Business.Similarity;

internal class FileSystemSimilarityDataSerializer : ISimilarityDataSerializer
{
    private const int BufferSize = 128 * 1024; // 128 KiB

    private readonly string _baseLocation;
    private readonly ILogger<FileSystemSimilarityDataSerializer> _logger;

    public FileSystemSimilarityDataSerializer(string baseLocation, ILogger<FileSystemSimilarityDataSerializer> logger)
    {
        _baseLocation = baseLocation;
        _logger = logger;
    }

    public async Task<ICollection<SimilarityDataDump>> GetDumpsAsync()
    {
        var dumps = new List<SimilarityDataDump>();
        foreach (var metadataLocation in Directory.GetFiles(_baseLocation, "*.json"))
        {
            try
            {
                await using var metadataFileStream = File.OpenRead(metadataLocation);
                var dump = await JsonSerializer.DeserializeAsync<SimilarityDataDump>(metadataFileStream);
                dumps.Add(dump!);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Something went wrong trying to read data dump metadata at {location}", metadataLocation);
            }
        }

        return dumps;
    }

    public async Task<SimilarityDataDump> CreateDumpAsync(ICollection<PlatformSimilarityData> items)
    {
        var dumpWhen = DateTime.UtcNow;
        var dumpId = Guid.NewGuid().ToString();

        _logger.LogInformation("Creating similarity data dump with ID {id}", dumpId);

        var platformDumps = new List<PlatformSimilarityDataDump>();
        foreach (var item in items)
        {
            var sfwDump = await DumpHashCollectionAsync(item.SfwCollection, dumpId, item.PlatformId, "sfw");
            var nsfwDump = await DumpHashCollectionAsync(item.NsfwCollection, dumpId, item.PlatformId, "nsfw");

            platformDumps.Add(new PlatformSimilarityDataDump
            {
                PlatformId = item.PlatformId,
                ChangeId = item.ChangeId,
                Sfw = sfwDump,
                Nsfw = nsfwDump
            });
        }

        var dump = new SimilarityDataDump
        {
            Id = dumpId,
            When = dumpWhen,
            Platforms = platformDumps
        };

        await using var metadataFileStream = new FileStream(GetLocation(GetDumpMetadataFileName(dumpId)), FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, FileOptions.WriteThrough);
        await JsonSerializer.SerializeAsync(metadataFileStream, dump);

        return dump;
    }

    private async Task<HashCollectionPlatformSimilarityDataDump> DumpHashCollectionAsync(IHashCollection collection, string dumpId, int platformId, string tag)
    {
        var fileName = $"{dumpId}_{platformId}_{tag}.bin";
        var path = GetLocation(fileName);
        _logger.LogInformation("Dumping {tag} hash collections for platform with ID {platformId} to {path}", tag, platformId, path);

        var stopwatch = Stopwatch.StartNew();

        var md5 = MD5.Create();
        await using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, FileOptions.WriteThrough))
        await using (var cryptoStream = new CryptoStream(fileStream, md5, CryptoStreamMode.Write))
            await collection.SerializeAsync(cryptoStream);

        _logger.LogInformation("Dump completed in {time}ms", stopwatch.ElapsedMilliseconds);

        var dump = new HashCollectionPlatformSimilarityDataDump
        {
            FileName = fileName,
            Md5 = md5.GetHashString()
        };
        return dump;
    }

    public async Task<ICollection<PlatformSimilarityData>> RestoreDumpAsync(SimilarityDataDump dump)
    {
        var data = new List<PlatformSimilarityData>();
        foreach (var platform in dump.Platforms)
        {
            var sfwCollection = await DeserializeHashCollectionAsync(platform.Sfw);
            var nsfwCollection = await DeserializeHashCollectionAsync(platform.Nsfw);

            data.Add(new PlatformSimilarityData
            {
                PlatformId = platform.PlatformId,
                ChangeId = platform.ChangeId,
                SfwCollection = sfwCollection,
                NsfwCollection = nsfwCollection
            });
        }

        return data;
    }

    private async Task<IHashCollection> DeserializeHashCollectionAsync(HashCollectionPlatformSimilarityDataDump dump)
    {
        var collection = HashCollectionFactory.Create();

        var md5 = MD5.Create();
        var dumpLocation = Path.Join(_baseLocation, dump.FileName);
        await using (var fileStream = new FileStream(dumpLocation, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize))
        await using (var cryptoStream = new CryptoStream(fileStream, md5, CryptoStreamMode.Read))
        {
            await collection.DeserializeAsync(cryptoStream);
        }
        md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        if (dump.Md5 != md5.GetHashString())
            throw new InvalidOperationException($"Hash collection read from {dumpLocation} does not match recorded MD5 hash. This might indicate that the dump is corrupted.");

        return collection;
    }

    public Task TryPurgeDumpAsync(SimilarityDataDump dump)
    {
        foreach (var platform in dump.Platforms)
        {
            TryDelete(platform.Sfw.FileName);
            TryDelete(platform.Nsfw.FileName);
        }

        TryDelete(GetDumpMetadataFileName(dump.Id));

        _logger.LogInformation("Deleted files related to dump with ID {id}", dump.Id);

        return Task.CompletedTask;
    }

    private void TryDelete(string filename)
    {
        var location = GetLocation(filename);
        try
        {
            File.Delete(location);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "File at {location} could not be deleted. Ignoring", location);
        }
    }

    private static string GetDumpMetadataFileName(string dumpId) => $"{dumpId}.json";

    private string GetLocation(string fileName) => Path.Join(_baseLocation, fileName);
}
