using System;
using System.Collections.Generic;
using System.Text;

namespace Models;
class Connection
{
    string ConnectionId { get; set; }
    string PlayerId { get; set; }

    Connection(string _connectionId, string _playerId)
    {
        ConnectionId = _connectionId;
        PlayerId = _playerId;
    }
}
