using Humanizer;
using Noppes.Fluffle.Queue;
using Noppes.Fluffle.Service;
using Noppes.Fluffle.Twitter.Client;
using Noppes.Fluffle.Twitter.Core;
using Noppes.Fluffle.Twitter.Core.Services;
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

    private readonly IUserService _userService;
    private readonly ITwitterApiClient _twitterApiClient;
    private readonly TwitterContext _twitterContext;
    private readonly IQueue<UserCheckFurryQueueItem> _userFurryCheckQueue;

    public Program(IServiceProvider services, IUserService userService, ITwitterApiClient twitterApiClient,
        TwitterContext twitterContext, IQueue<UserCheckFurryQueueItem> userFurryCheckQueue) : base(services)
    {
        _userService = userService;
        _twitterApiClient = twitterApiClient;
        _twitterContext = twitterContext;
        _userFurryCheckQueue = userFurryCheckQueue;
    }

    public override async Task ProcessAsync(ImportUserQueueItem userToImport, CancellationToken cancellationToken)
    {
        Log.Information("Start processing user with username @{username}", userToImport.Username);

        // First we check if an active user already exists with the same username
        var existingUsers = await _twitterContext.Users.ManyAsync(x => x.Username == userToImport.Username, true);
        if (existingUsers.Any(x => !x.IsDeleted))
        {
            Log.Information("Skipping import for @{username} because there already exists an active user with that username", userToImport.Username);
            return;
        }

        // If not then we check if the import failed before
        var existingImportFailure = await _twitterContext.UserImportFailures.FirstOrDefaultAsync(x => x.Username == userToImport.Username, true);
        if (existingImportFailure != null && DateTime.UtcNow.Subtract(existingImportFailure.ImportedAt) < TimeSpan.FromDays(90)) // Retry after 90 days
        {
            Log.Information("Skipping import for @{username} because the import failed less than 90 days ago", userToImport.Username);
            return;
        }

        try
        {
            var userModel = await _twitterApiClient.GetUserByUsernameAsync(userToImport.Username);
            var userEntity = await _twitterContext.Users.FirstOrDefaultAsync(x => x.Id == userModel.RestId);
            if (userEntity != null)
            {
                Log.Information("User @{username} ({id}) has already been imported before, updating information", userModel.Username, userModel.RestId);
                await _userService.UpdateDetailsAsync(userEntity, userModel);

                return;
            }

            userEntity = new UserEntity
            {
                Id = userModel.RestId,
                AlternativeId = userModel.Id,
                IsProtected = userModel.IsProtected,
                IsSuspended = false,
                IsDeleted = false,
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
                Id = userEntity.Id
            }, userEntity.FollowersCount, 1.Minutes(), null);

            // Add said user to the database
            await _twitterContext.Users.InsertAsync(userEntity);

            Log.Information("Successfully imported @{username}", userToImport.Username);
        }
        catch (TwitterUserException e)
        {
            Log.Information("User {username} could not be imported for the following reason: {reason}", userToImport.Username, e.Error.Reason);
            if (existingImportFailure == null)
            {
                await _twitterContext.UserImportFailures.InsertAsync(new UserImportFailureEntity
                {
                    Username = userToImport.Username,
                    ImportedAt = DateTime.UtcNow,
                    Reason = e.Error.Reason.ToString()
                });
            }
            else
            {
                existingImportFailure.ImportedAt = DateTime.UtcNow;
                existingImportFailure.Reason = e.Error.Reason.ToString();
                await _twitterContext.UserImportFailures.UpsertAsync(x => x.Id == existingImportFailure.Id, existingImportFailure);
            }
        }
    }
}
