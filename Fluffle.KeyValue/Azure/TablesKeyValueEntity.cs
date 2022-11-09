using Azure;
using Azure.Data.Tables;

namespace Noppes.Fluffle.KeyValue.Azure;

internal class TablesKeyValueEntity : ITableEntity
{
    public string PartitionKey { get; set; } = null!;

    public string RowKey { get; set; } = null!;

    public string Value { get; set; } = null!;

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }
}