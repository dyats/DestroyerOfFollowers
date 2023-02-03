using DestroyerOfFollowers.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

Console.OutputEncoding = Encoding.UTF8;

#region Client configuration

var appClient = new TwitterClient("CONSUMER_KEY", "CONSUMER_SECRET");

// Start the authentication process
var authenticationRequest = await appClient.Auth.RequestAuthenticationUrlAsync();

// Go to the URL so that Twitter authenticates the user and gives him a PIN code.
Process.Start(new ProcessStartInfo(authenticationRequest.AuthorizationURL)
{
    UseShellExecute = true
});

// Ask the user to enter the pin code given by Twitter
Console.WriteLine("Please enter the code and press enter.");
var pinCode = Console.ReadLine();

await appClient.Auth.InitializeClientBearerTokenAsync();
// With this pin code it is now possible to get the credentials back from Twitter
var userCredentials = await appClient.Auth.RequestCredentialsFromVerifierCodeAsync(pinCode, authenticationRequest);

userCredentials.BearerToken = await appClient.Auth.CreateBearerTokenAsync();

// You can now save those credentials or use them as followed
var userClient = new TwitterClient(userCredentials);
var user = await userClient.Users.GetAuthenticatedUserAsync();

Console.WriteLine("Congratulation you have authenticated the user: " + user);
#endregion

#region User timeline tweets
Console.WriteLine("Get user timeline");

var userTimelineParameters = new GetUserTimelineParameters(user)
{
    ExcludeReplies = true,
};
var timelineIterator = userClient.Timelines.GetUserTimelineIterator(userTimelineParameters);
var timelineTweets = new List<ITweet>();

while (!timelineIterator.Completed)
{
    var page = await timelineIterator.NextPageAsync();
    timelineTweets.AddRange(page.ToArray());
}
timelineTweets = timelineTweets.Where(x => x.CreatedAt > new DateTime(2023, 01, 18)).ToList();

var timeLineTweetsIds = timelineTweets.Select(x => (Id: x.Id, CreatedAt: x.CreatedAt)).ToList();

#endregion

#region My direct replies(thread) to user timeline tweets
Console.WriteLine("Get direct replies from user(thread replies)");

var threadRepliesIds = new List<(long, DateTimeOffset)>(); 
foreach (var tweet in timeLineTweetsIds)
{
    // thread replies information
    var getRepliesInfoUrl = $@"https://api.twitter.com/2/tweets/search/recent?query=from:{user.Id} to:{user.Id} conversation_id:{tweet.Id}&tweet.fields=in_reply_to_user_id,author_id,created_at,conversation_id&max_results=100";

    var getRepliesRateLimit = await userClient.RateLimits.GetEndpointRateLimitAsync(getRepliesInfoUrl);

    var replies = await userClient.Execute.RequestAsync(request =>
    {
        request.Url = getRepliesInfoUrl;
        request.HttpMethod = Tweetinvi.Models.HttpMethod.GET;
        request.AuthorizationHeader = $"Bearer {userCredentials.BearerToken}";
    });

    var threadReplyIds = JsonConvert.DeserializeObject<ReplyMetadata>(replies.Content).Data?
        .Select(x => (Id: Convert.ToInt64(x.Id), CreatedAt: new DateTimeOffset(x.Created_At)))
        .Distinct();

    if (threadReplyIds?.Any() ?? false)
    {
        threadRepliesIds.AddRange(threadReplyIds);
    }
}

timeLineTweetsIds.AddRange(threadRepliesIds);

#endregion

timeLineTweetsIds = timeLineTweetsIds.OrderByDescending(x => x.CreatedAt).ToList();

// we take only 75 tweets because Twitter API has reate limitation on liking_users endpoint(75 requests per 15 mins)
var tweets = await userClient.TweetsV2.GetTweetsAsync(timeLineTweetsIds.Take(75).Select(x => x.Id).ToArray());

var userIdsThatInteracted = new List<long>();

#region Replies to tweets
Console.WriteLine("Get all replies for last 7 days");

// fetch all replies for last 7 days
var mentionsTimelineParameters = new GetMentionsTimelineParameters()
{
    PageSize = 100,
    IncludeEntities = true,
    IncludeContributorDetails = true,
};
mentionsTimelineParameters.AddCustomQueryParameter("start_time", DateTime.Now.AddDays(-7).ToString("yyyy-MM-DDTHH:mm:ssZ"));
mentionsTimelineParameters.AddCustomQueryParameter("from", user.Id.ToString());

var mentionsIterator = userClient.Timelines.GetMentionsTimelineIterator(mentionsTimelineParameters);
var timelineMentions = new List<ITweet>();
while (!mentionsIterator.Completed)
{
    var page = await mentionsIterator.NextPageAsync();
    timelineMentions.AddRange(page.ToArray());
}
timelineMentions = timelineMentions.Where(x => timeLineTweetsIds.Select(x => x.Id).Contains(x.InReplyToStatusId ?? 0)).ToList();
var usersThatReplied = timelineMentions.Select(x => x.CreatedBy.Id).Distinct().ToList();

userIdsThatInteracted.AddRange(usersThatReplied);
#endregion

#region Who liked tweets
Console.WriteLine("Check who liked last 75 tweets");

foreach (var tweet in tweets.Tweets)
{
    // likes information
    var getUsersThatLikedUrl = $@"https://api.twitter.com/2/tweets/{tweet.Id}/liking_users";

    var getUsersThatLikeRateLimit = await userClient.RateLimits.GetEndpointRateLimitAsync(getUsersThatLikedUrl, RateLimitsSource.TwitterApiOnly);

    var likingUsers = await userClient.Execute.RequestAsync(request =>
    {
        request.Url = getUsersThatLikedUrl;
        request.HttpMethod = Tweetinvi.Models.HttpMethod.GET;
    });

    var likedUserIds = JsonConvert.DeserializeObject<LikesMetadata>(likingUsers.Content).Data?.Select(x => Convert.ToInt64(x.Id));

    // add unique ids to the main list
    userIdsThatInteracted.AddRange(likedUserIds ?? Enumerable.Empty<long>());
}
#endregion

#region Distinguish users
Console.WriteLine("Remove followers");

userIdsThatInteracted = userIdsThatInteracted.Distinct().ToList();
var myFollowers = await userClient.Users.GetFollowerIdsAsync(user);
var usersToRemove = myFollowers.Except(userIdsThatInteracted);

var usersToRemoveExcept = new List<long>
{
    /*specify user ids that should not be removed*/
};

usersToRemove = usersToRemove.Except(usersToRemoveExcept);

var counterOnRemovedFollowers = 0;
for (int i = 0; i < usersToRemove.Count(); i += 99)
{
    var userss = await userClient.UsersV2.GetUsersByIdAsync(usersToRemove.Skip(i).Take(99).ToArray());
    foreach (var u in userss.Users)
    {
        await userClient.Users.BlockUserAsync(u.Username);
        await userClient.Users.UnblockUserAsync(u.Username);
        Console.WriteLine($"User {u.Username} was removed from followers, Link: https://twitter.com/{u.Username}");
        counterOnRemovedFollowers++;
    }
}
Console.WriteLine($"Removed followers: {counterOnRemovedFollowers}");
#endregion

Console.ReadLine();
