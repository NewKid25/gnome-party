using Amazon.DynamoDBv2.DataModel;

namespace GnomeParty.Models;

[DynamoDBTable("GameTable")]
public class GameSession
{
    [DynamoDBHashKey]
    public string GameSessionId { get; set; }
    public GameConnection Host { get; set; }
    public List<GameConnection> Participants;

    [DynamoDBGlobalSecondaryIndexHashKey("InviteCode-index")]
    public int InviteCode { get; set; }
    public Campaign Campaign { get; set; }
    //[DynamoDBProperty]
    //public Character character { get; set; }


    public GameSession() 
    {
        GameSessionId = Guid.NewGuid().ToString();
        InviteCode = 0;
        //character = new Character();
        Host = new GameConnection();
        Participants = new List<GameConnection>();
        Campaign = new Campaign();
    }

    public GameSession(GameConnection _host)
    {
        GameSessionId = Guid.NewGuid().ToString();
        Host = _host;
        Participants = new List<GameConnection>();
        InviteCode = 0;
        Campaign = new Campaign();
        //character = new Character();
    }

}
