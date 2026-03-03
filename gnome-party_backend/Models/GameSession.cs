using Amazon.DynamoDBv2.DataModel;

namespace GnomeParty.Models;

[DynamoDBTable("GameTable")]
public class GameSession
{
    [DynamoDBHashKey]
    public string GameSessionId { get; set; }
    public GameConnection Host { get; set; }
    public List<GameConnection> Participants { get; set; }

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

    public void AddParticipant(string connectionId, string userId)
    {
        var connection = new GameConnection(connectionId, userId);
        AddParticipant(connection);
    }

    public void AddParticipant(GameConnection connection)
    {
        Participants.Add(connection);
        var character = new Character(connection.UserId);
        Campaign.PlayerCharacters.Add(character);
    }
}
