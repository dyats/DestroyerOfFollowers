# Destroyer Of Followers

Destroyer Of Followers is a script specifically designed to unfollow Twitter users who haven't interacted with you recently. This automated process keeps your Twitter community engaged and relevant.

## Limitations

- The script checks only replies to your tweets from the last 7 days.
- It also checks likes on your last 75 tweets, including your replies to the main tweet.

## Application Flow

1. **Authorization**: On launching the application, you'll be prompted to authorize in your browser. Twitter will then display a PIN code which you should input into the console.
2. **Timeline Retrieval**: The application fetches user timeline tweets from the last month.
3. **Replies Extraction**: It retrieves direct replies to those tweets from the authorized user (main thread replies).
4. **Tweet Limit**: Only the last 75 tweets are considered due to the API's limitation for requests that read who liked your tweets.
5. **Interaction Checks**: The application collects all replies to these tweets from the last 7 days and checks who liked your last 75 tweets.
6. **Unfollowing Process**: It identifies which followers are not present in the list of users that interacted with you, then proceeds to block and unblock these users, effectively unfollowing them.

Maintain a healthy and engaged Twitter community with the Destroyer Of Followers.
