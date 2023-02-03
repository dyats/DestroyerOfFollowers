using Newtonsoft.Json;

namespace DestroyerOfFollowers.Models;

public class LikesData
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }
}
