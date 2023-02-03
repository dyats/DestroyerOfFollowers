using Newtonsoft.Json;

namespace DestroyerOfFollowers.Models;

public class MetaData<T> where T : class
{
    [JsonProperty("data")]
    public List<T>? Data { get; set; }

    [JsonProperty("meta")]
    public Meta Meta { get; set; }
}