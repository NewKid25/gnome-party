using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace GnomeParty.Models;

[DynamoDBTable("ConnectionsTable")]
public class GameConnection
{
    [DynamoDBHashKey]
    public string ConnectionId { get; set; }
    public string UserId { get; set; }

    public GameConnection() 
    {
        ConnectionId = "uninitialized_connection_id";
        UserId = "not_inited";
    }

    public GameConnection(string connectionId, string userId = "not_inited")
    {
        ConnectionId = connectionId;
        UserId = userId;
    }
}
