using Fluffle.Ingestion.Api.Models.Items;
using System.Text.Json.Serialization;

namespace Fluffle.Ingestion.Api.Models.ItemActions;

[JsonDerivedType(typeof(IndexItemActionModel), "index")]
public class IndexItemActionModel : ItemActionModel
{
    public required ItemModel Item { get; set; }

    public override T Visit<T>(IItemActionModelVisitor<T> visitor) => visitor.Visit(this);
}
