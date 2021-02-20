using System;

namespace Noppes.Fluffle.Api.AccessControl
{
    /// <summary>
    /// Marks a public const string field as one which should be treated as a permission.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PermissionAttribute : Attribute
    {
    }
}
