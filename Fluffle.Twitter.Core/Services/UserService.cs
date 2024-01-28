using MongoDB.Driver;
using Noppes.Fluffle.Twitter.Client;
using Noppes.Fluffle.Twitter.Database;

namespace Noppes.Fluffle.Twitter.Core.Services;

public interface IUserService
{
    Task<UserEntity> UpdateDetailsAsync(UserEntity user);
    Task<UserEntity> UpdateDetailsAsync(UserEntity user, TwitterUserModel userModel);
}

internal class UserService : IUserService
{
    private readonly ITwitterApiClient _twitterApiClient;
    private readonly TwitterContext _twitterContext;

    public UserService(ITwitterApiClient twitterApiClient, TwitterContext twitterContext)
    {
        _twitterApiClient = twitterApiClient;
        _twitterContext = twitterContext;
    }

    public async Task<UserEntity> UpdateDetailsAsync(UserEntity user)
    {
        UpdateDefinition<UserEntity> update;
        try
        {
            var userModel = await _twitterApiClient.GetUserByIdAsync(user.Id);
            update = GetUpdateDefinition(userModel);
        }
        catch (TwitterUserException e)
        {
            update = GetUpdateDefinition(e);
        }

        var updatedUser = await UpdateUserAsync(user, update);
        return updatedUser;
    }

    public async Task<UserEntity> UpdateDetailsAsync(UserEntity user, TwitterUserModel userModel)
    {
        var update = GetUpdateDefinition(userModel);
        var updatedUser = await UpdateUserAsync(user, update);

        return updatedUser;
    }

    private async Task<UserEntity> UpdateUserAsync(UserEntity user, UpdateDefinition<UserEntity> update)
    {
        var filter = Builders<UserEntity>.Filter.Eq(x => x.Id, user.Id);
        await _twitterContext.Users.Collection.FindOneAndUpdateAsync(filter, update);
        var updatedUser = await _twitterContext.Users.FirstAsync(x => x.Id == user.Id);

        return updatedUser;
    }

    private static UpdateDefinition<UserEntity> GetUpdateDefinition(TwitterUserModel userModel)
    {
        var update = Builders<UserEntity>.Update
            .Set(x => x.IsProtected, userModel.IsProtected)
            .Set(x => x.IsSuspended, false)
            .Set(x => x.IsDeleted, false)
            .Set(x => x.HasViolatedMediaPolicy, false)
            .Set(x => x.Description, userModel.Description)
            .Set(x => x.FollowersCount, userModel.FollowersCount)
            .Set(x => x.FollowingCount, userModel.FollowingCount)
            .Set(x => x.Name, userModel.Name)
            .Set(x => x.ProfileBannerUrl, userModel.ProfileBannerUrl)
            .Set(x => x.ProfileImageUrl, userModel.ProfileImageUrl)
            .Set(x => x.Username, userModel.Username);

        return update;
    }

    private static UpdateDefinition<UserEntity> GetUpdateDefinition(TwitterUserException exception)
    {
        var update = Builders<UserEntity>.Update
            .Set(x => x.IsSuspended, exception.Error.Reason == TwitterUserError.Suspended)
            .Set(x => x.IsDeleted, exception.Error.Reason == TwitterUserError.NotFound)
            .Set(x => x.HasViolatedMediaPolicy, exception.Error.Reason == TwitterUserError.MediaPolicyViolated);

        return update;
    }
}
