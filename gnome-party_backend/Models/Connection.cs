namespace Models;
public class Connection
{
    string ConnectionId { get; set; }
    string PlayerId { get; set; }

    public Connection(string _connectionId, string _playerId)
    {
        ConnectionId = _connectionId;
        PlayerId = _playerId;
    }
}
