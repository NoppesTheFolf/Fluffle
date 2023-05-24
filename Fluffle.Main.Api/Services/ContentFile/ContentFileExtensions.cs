using Noppes.Fluffle.Database.Synchronization;
using Noppes.Fluffle.Main.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services;

public static class ContentFileExtensions
{
    public static async Task<SynchronizeResult<ContentFile>> SynchronizeFilesAsync(this FluffleContext context, ICollection<ContentFile> currentFiles, ICollection<ContentFile> newFiles)
    {
        return await context.SynchronizeAsync(c => c.ContentFiles, currentFiles, newFiles, (f1, f2) =>
        {
            return (f1.Content, f1.Location) == (f2.Content, f2.Location);
        }, onUpdateAsync: (src, dest) =>
        {
            dest.FileFormatId = src.FileFormatId;
            dest.Width = src.Width;
            dest.Height = src.Height;

            return Task.CompletedTask;
        });
    }
}
