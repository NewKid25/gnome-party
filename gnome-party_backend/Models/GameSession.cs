using Amazon.DynamoDBv2.DataModel;

namespace Models;

[DynamoDBTable("GameTable")]
public class GameSession
{
    [DynamoDBHashKey]
    public string gameSessionId { get; set; }
    //Connection Host { get; set; }
    //List<Connection> Participants;
    public int InviteCode { get; set; }
    //Campaign Campaign { get; set; }
    [DynamoDBProperty]
    public Character character { get; set; }


    public GameSession() 
    {
        gameSessionId = Guid.NewGuid().ToString();
        InviteCode = 0;
        character = new Character();
    }

    public GameSession(Connection _host)
    {
        gameSessionId = Guid.NewGuid().ToString();
        //Host = _host;
        //Participants = new List<Connection>();
        InviteCode = 0;
        //Campaign = new Campaign();
        character = new Character();
    }

}
