namespace Noppes.Fluffle.Search.Business.Similarity;

internal interface ISimilarityDataSerializer
{
    Task<SimilarityDataDump> CreateDumpAsync(ICollection<PlatformSimilarityData> items);

    Task<ICollection<SimilarityDataDump>> GetDumpsAsync();

    Task<ICollection<PlatformSimilarityData>> RestoreDumpAsync(SimilarityDataDump dump);

    Task TryPurgeDumpAsync(SimilarityDataDump dump);
}
