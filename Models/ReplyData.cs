using Newtonsoft.Json;

namespace DestroyerOfFollowers.Models;

public class ReplyData
{
    [JsonProperty("in_reply_to_user_id")]
    public string InReplyToUserId { get; set; }

    [JsonProperty("edit_history_tweet_ids")]
    public List<string> EditHistoryTweetIds { get; set; }

    [JsonProperty("author_id")]
    public string AuthorId { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("conversation_id")]
    public string ConversationId { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }
}
