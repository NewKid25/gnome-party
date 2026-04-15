using Amazon.DynamoDBv2.DataModel;
using Models.CharacterData;

namespace Models.GameMetaData;

[DynamoDBTable("GameTable")]
public class GameSession
{
    [DynamoDBHashKey]
    public string GameSessionId { get; set; }

    [DynamoDBGlobalSecondaryIndexHashKey("InviteCode-index")]
    public int InviteCode { get; set; }
    
    public Campaign Campaign { get; set; }
    public GameConnection Host { get; set; }
    public GameSession() : this(new GameConnection()) { }
    public GameSession(GameConnection _host)
    {
        GameSessionId = Guid.NewGuid().ToString();
        Host = _host;
        Participants = new List<GameConnection>();
        InviteCode = Random.Shared.Next(100000, 1000000); //random 6 digit code
        Campaign = new Campaign();
    }
    public List<GameConnection> Participants { get; set; }
    public void AddParticipant(GameConnection connection)
    {
        Participants.Add(connection);
    }
    public void AddPlayerCharacter(Character character)
    {
        Campaign.PlayerCharacters.Add(character);
    }

    public void RemoveParticipant(string connectionId)
    {
        var connection = Participants.FirstOrDefault(c => c.ConnectionId == connectionId);
        if (connection != null)
        {
            Participants.Remove(connection);
            RemovePlayerCharacter(connectionId);
        }
    }

    public void RemovePlayerCharacter(string characterId)
    {
        var character = Campaign.PlayerCharacters.FirstOrDefault(pc => pc.Id == characterId);
        if (character != null)
        {
            Campaign.PlayerCharacters.Remove(character);
        }
    }
}
