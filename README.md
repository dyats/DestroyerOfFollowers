# Destroyer Of Followers
Script created to unfollow followers from twitter in case we haven't communicated for a while.

## Limitations
- Checks only replies to your tweets for last 7 days
- Checks only likes on last 75 tweets(including you replies to main tweet)

## Application flow
1. When you'll launch the application you'll be asked to authorize in the browser and Twitter will show you the PIN code which you have to enter in the console
2. Get user timeline tweets (for last month)
3. Get direct replies to those tweets from authorized user(main thread replies)
4. Takes only last 75 tweets (limitation of the API for request that reads who liked your tweets, **take a look for point 6**)
5. Get all replies to these tweets for last 7 days
6. Check who liked last 75 tweets (limitation 75 requests for 15 mins)
7. Check which followers is not present in this list of users that interacted with me
8. Block -> Unblock user
