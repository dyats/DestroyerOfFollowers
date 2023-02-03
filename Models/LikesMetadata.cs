namespace DestroyerOfFollowers.Models;

public class LikesMetadata
{
    public List<LikesData>? Data { get; set; }
    public Meta Meta { get; set; }
}

public class LikesData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }
}
