using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models;

[DynamoDBTable("ConnectionsTable")]
public class GameConnection
{
    [DynamoDBHashKey]
    public string ConnectionId { get; set; }
    public string UserId { get; set; }
    public string GameSessionId { get; set; }
    public GameConnection(string connectionId, string userId="not_inited", string gameSessionId = "not_inited")
    {
        ConnectionId = connectionId;
        UserId = userId;
        GameSessionId = gameSessionId;
    }

    public GameConnection(string connectionId, string userId, GameSession session): this (connectionId, userId, session.GameSessionId) { }

    public GameConnection() : this ("uninitialized_connection_id") { }

}
