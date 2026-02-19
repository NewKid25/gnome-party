using System;
using System.Collections.Generic;
using System.Text;

namespace Models;
class GameSession
{
    Connection Host { get; set; }
    Connection[] Participants;
    int InviteCode { get; set; }
    

}
