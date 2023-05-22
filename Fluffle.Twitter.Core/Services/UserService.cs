using MongoDB.Driver;
using Noppes.Fluffle.Twitter.Client;
using Noppes.Fluffle.Twitter.Database;

namespace Noppes.Fluffle.Twitter.Core.Services;

public interface IUserService
{
    Task<UserEntity> UpdateDetailsAsync(UserEntity user);
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
            var userModel = await _twitterApiClient.GetUserAsync(user.Username);

            update = Builders<UserEntity>.Update
                .Set(x => x.IsProtected, userModel.IsProtected)
                .Set(x => x.Description, userModel.Description)
                .Set(x => x.FollowersCount, userModel.FollowersCount)
                .Set(x => x.FollowingCount, userModel.FollowingCount)
                .Set(x => x.Name, userModel.Name)
                .Set(x => x.ProfileBannerUrl, userModel.ProfileBannerUrl)
                .Set(x => x.ProfileImageUrl, userModel.ProfileImageUrl)
                .Set(x => x.Username, userModel.Username);
        }
        catch (TwitterUserException e)
        {
            update = Builders<UserEntity>.Update
                .Set(x => x.IsSuspended, e.Error.Reason == TwitterUserError.Suspended)
                .Set(x => x.IsDeleted, e.Error.Reason == TwitterUserError.NotFound);
        }

        var filter = Builders<UserEntity>.Filter.Eq(x => x.Id, user.Id);
        await _twitterContext.Users.Collection.FindOneAndUpdateAsync(filter, update);
        var updatedUser = await _twitterContext.Users.FirstAsync(x => x.Id == user.Id);

        return updatedUser;
    }
}
