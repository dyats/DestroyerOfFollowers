namespace DestroyerOfFollowers.Models;

public class LikesMetadata
{
    public List<LikesData>? Data { get; set; }
    public LikesMeta Meta { get; set; }
}

public class LikesData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }
}

public class LikesMeta
{
    public int Result_Count { get; set; }
    public string Newest_Id { get; set; }
    public string Oldest_Id { get; set; }
    public string Next_Token { get; set; }
    public string Previous_Token { get; set; }
}
