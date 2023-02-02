using DestroyerOfFollowers.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

Console.OutputEncoding = Encoding.UTF8;

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

var myFollowers = await userClient.Users.GetFollowerIdsAsync(user);

Console.WriteLine("Congratulation you have authenticated the user: " + user);

var userTimelineParameters = new GetUserTimelineParameters(user)
{
    ExcludeReplies = true,
    PageSize = 100
};
userTimelineParameters.AddCustomQueryParameter("start_time", DateTime.Now.AddDays(-7).ToString("yyyy-MM-DDTHH:mm:ssZ"));

var mentionsTimelineParameters = new GetMentionsTimelineParameters()
{
    PageSize = 100,
    IncludeEntities = true,
    IncludeContributorDetails = true,
};
mentionsTimelineParameters.AddCustomQueryParameter("start_time", DateTime.Now.AddDays(-7).ToString("yyyy-MM-DDTHH:mm:ssZ"));
mentionsTimelineParameters.AddCustomQueryParameter("from", user.Id.ToString());

var timelineIterator = userClient.Timelines.GetUserTimelineIterator(userTimelineParameters);
var timelineTweets = new List<ITweet>();

while (!timelineIterator.Completed)
{
    var page = await timelineIterator.NextPageAsync();
    timelineTweets.AddRange(page.ToArray());
}

var timeLineTweetsIds = timelineTweets.Take(75).Select(x => x.Id).ToList();
//foreach (var tweetId in timeLineTweetsIds)
//{
//    // thread replies information
//    var getRepliesInfoUrl = $@"https://api.twitter.com/2/tweets/search/recent?query=from:{user.Id} to:{user.Id} conversation_id:{tweetId}&tweet.fields=in_reply_to_user_id,author_id,created_at,conversation_id&max_results=100";

//    var getRepliesRateLimit = await userClient.RateLimits.GetEndpointRateLimitAsync(getRepliesInfoUrl);

//    var replies = await userClient.Execute.RequestAsync(request =>
//    {
//        request.Url = getRepliesInfoUrl;
//        request.HttpMethod = Tweetinvi.Models.HttpMethod.GET;
//        request.AuthorizationHeader = $"Bearer {userCredentials.BearerToken}";
//    });

//    var threadReplyIds = JsonConvert.DeserializeObject<ReplyMetadata>(replies.Content).Data?
//        .Select(x => Convert.ToInt64(x.Id))
//        .Distinct();

//    timeLineTweetsIds.AddRange(threadReplyIds ?? Enumerable.Empty<long>());
//}



var tweets = await userClient.TweetsV2.GetTweetsAsync(timeLineTweetsIds.ToArray());

// fetch all replies for last 7 days
var mentionsIterator = userClient.Timelines.GetMentionsTimelineIterator(mentionsTimelineParameters);
var timelineMentions = new List<ITweet>();
while (!mentionsIterator.Completed)
{
    var page = await mentionsIterator.NextPageAsync();
    timelineMentions.AddRange(page.ToArray());
}

timelineMentions = timelineMentions.Where(x => timeLineTweetsIds.Contains(x.InReplyToStatusId ?? 0)).ToList();

var userIdsThatInteracted = new List<long>();
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

userIdsThatInteracted = userIdsThatInteracted.Distinct().ToList();

//var usersToRemove = myFollowers.Except(userIdsThatInteracted);

//for (int i = 0; i < usersToRemove.Count(); i += 99)
//{
//    var userss = await userClient.UsersV2.GetUsersByIdAsync(usersToRemove.Skip(i).Take(99).ToArray());
//    foreach (var u in userss.Users)
//    {
//        Console.WriteLine($"Id: {u.Id}, UserName: {u.Username}, Name: {u.Name}, Link: {u.Url}");
//    }
//}

Console.ReadLine();
