using System;
using System.Collections.Generic;
using System.Text;

namespace Models;
public class GameSession
{
    Connection Host { get; set; }
    List<Connection> Participants;
    int InviteCode { get; set; }
    Campaign Campaign { get; set; }

    public GameSession(Connection _host)
    {
        Host = _host;
        Participants = new List<Connection>();
        InviteCode = 0;
        Campaign = new Campaign();
    }

}
