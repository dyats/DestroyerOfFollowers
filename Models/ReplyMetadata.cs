namespace DestroyerOfFollowers.Models;

public class ReplyMetadata
{
    public List<ReplyData>?  Data { get; set; }
    public Meta Meta { get; set; }
}

public class ReplyData
{
    public string In_Reply_To_User_Id { get; set; }
    public List<string> Edit_History_Tweet_Ids { get; set; }
    public string Author_Id { get; set; }
    public string Id { get; set; }
    public DateTime Created_At { get; set; }
    public string Conversation_Id { get; set; }
    public string Text { get; set; }
}
