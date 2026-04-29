using Amazon.DynamoDBv2.DataModel;

namespace Models.GameMetaData;

[DynamoDBTable("ConnectionsTable")]
public class GameConnection
{
    [DynamoDBHashKey]
    public string ConnectionId { get; set; }
    public string GameSessionId { get; set; }
    [DynamoDBGlobalSecondaryIndexHashKey("UserId-index")]
    public string UserId { get; set; } //same as character's id if this is a participant
    public GameConnection() : this ("uninitialized_connection_id") { }
    public GameConnection(string connectionId, string userId="not_inited", string gameSessionId = "not_inited")
    {
        ConnectionId = connectionId;
        UserId = userId;
        GameSessionId = gameSessionId;
    }
    public GameConnection(string connectionId, string userId, GameSession session): this (connectionId, userId, session.GameSessionId) { }

}
