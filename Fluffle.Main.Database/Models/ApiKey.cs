using Noppes.Fluffle.Api.AccessControl;
using System.Collections.Generic;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Models;

public partial class ApiKey : ApiKey<ApiKey, Permission, ApiKeyPermission>
{
    public ApiKey()
    {
        LastEditedContent = new HashSet<Content>();
    }

    public virtual ICollection<Content> LastEditedContent { get; set; }
}
