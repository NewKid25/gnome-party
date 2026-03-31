using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models.GameMetaData;

[DynamoDBTable("ConnectionsTable")]
public class GameConnection
{
    [DynamoDBHashKey]
    public string ConnectionId { get; set; }
    public string GameSessionId { get; set; }
    public string UserId { get; set; }
    public GameConnection() : this ("uninitialized_connection_id") { }
    public GameConnection(string connectionId, string userId="not_inited", string gameSessionId = "not_inited")
    {
        ConnectionId = connectionId;
        UserId = userId;
        GameSessionId = gameSessionId;
    }
    public GameConnection(string connectionId, string userId, GameSession session): this (connectionId, userId, session.GameSessionId) { }

}
