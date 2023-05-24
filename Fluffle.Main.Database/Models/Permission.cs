using Noppes.Fluffle.Api.AccessControl;

#nullable disable

namespace Noppes.Fluffle.Main.Database.Models;

public partial class Permission : Permission<ApiKey, Permission, ApiKeyPermission>
{
}
