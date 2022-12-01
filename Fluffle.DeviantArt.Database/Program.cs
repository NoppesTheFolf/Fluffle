using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.DeviantArt.Database;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var context = new DeviantArtDesignTimeContext().CreateDbContext(args);

        Console.Write("Applying migrations...");
        await context.Database.MigrateAsync();
        Console.WriteLine(" OK!");
    }
}
