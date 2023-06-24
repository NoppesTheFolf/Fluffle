using Humanizer;
using Noppes.Fluffle.Queue;
using Noppes.Fluffle.Service;
using Noppes.Fluffle.Twitter.Client;
using Noppes.Fluffle.Twitter.Core;
using Noppes.Fluffle.Twitter.Database;
using Serilog;

namespace Noppes.Fluffle.Twitter.UserImporter;

internal class Program : QueuePollingService<Program, ImportUserQueueItem>
{
    protected override TimeSpan Interval => 5.Minutes();

    protected override TimeSpan VisibleAfter => 1.Hours();

    private static async Task Main(string[] args) => await RunAsync(args, "TwitterUserImporter", (conf, services) =>
    {
        services.AddCore(conf);
    });

    private readonly ITwitterApiClient _twitterApiClient;
    private readonly TwitterContext _twitterContext;
    private readonly IQueue<UserCheckFurryQueueItem> _userFurryCheckQueue;

    public Program(IServiceProvider services, ITwitterApiClient twitterApiClient, TwitterContext twitterContext, IQueue<UserCheckFurryQueueItem> userFurryCheckQueue) : base(services)
    {
        _twitterApiClient = twitterApiClient;
        _twitterContext = twitterContext;
        _userFurryCheckQueue = userFurryCheckQueue;
    }

    public override async Task ProcessAsync(ImportUserQueueItem userToImport, CancellationToken cancellationToken)
    {
        Log.Information("Start processing user with username @{username}", userToImport.Username);

        // First we check if the user has already been imported (successfully) before
        var existsAsUser = await _twitterContext.Users.AnyAsync(x => x.Username == userToImport.Username);
        var existsAsFailure = await _twitterContext.UserImportFailures.AnyAsync(x => x.Id == userToImport.Username);
        if (existsAsUser || existsAsFailure)
            return;

        try
        {
            var userModel = await _twitterApiClient.GetUserByUsernameAsync(userToImport.Username);
            if (await _twitterContext.Users.AnyAsync(x => x.Id == userModel.RestId))
            {
                Log.Warning("User @{username} ({id}) has already been imported before", userModel.Username, userModel.RestId);
                return;
            }

            var user = new UserEntity
            {
                Id = userModel.RestId,
                AlternativeId = userModel.Id,
                IsProtected = userModel.IsProtected,
                CreatedAt = userModel.CreatedAtParsed.ToUniversalTime(),
                Description = userModel.Description,
                FollowersCount = userModel.FollowersCount,
                FollowingCount = userModel.FollowingCount,
                Name = userModel.Name,
                ProfileBannerUrl = userModel.ProfileBannerUrl,
                ProfileImageUrl = userModel.ProfileImageUrl,
                Username = userModel.Username,
                ImportedAt = DateTime.UtcNow
            };

            // Schedule user to be checked whether they post furry art or not
            await _userFurryCheckQueue.EnqueueAsync(new UserCheckFurryQueueItem
            {
                Id = user.Id
            }, user.FollowersCount, 1.Minutes(), null);

            // Add said user to the database
            await _twitterContext.Users.InsertAsync(user);

            Log.Information("Successfully imported @{username}", userToImport.Username);
        }
        catch (TwitterUserException e)
        {
            Log.Information("User {username} could not be imported for the following reason: {reason}", userToImport.Username, e.Error.Reason);
            await _twitterContext.UserImportFailures.InsertAsync(new UserImportFailureEntity
            {
                Id = userToImport.Username,
                ImportedAt = DateTime.UtcNow,
                Reason = e.Error.Reason.ToString()
            });
        }
    }
}
