using Newtonsoft.Json;

namespace DestroyerOfFollowers.Models;

public class Meta
{
    [JsonProperty("id")]
    public int Result_Count { get; set; }

    [JsonProperty("newest_id")]
    public string NewestId { get; set; }

    [JsonProperty("oldest_id")]
    public string OldestId { get; set; }

    [JsonProperty("next_token")]
    public string NextToken { get; set; }

    [JsonProperty("previous_token")]
    public string PreviousToken { get; set; }
}
