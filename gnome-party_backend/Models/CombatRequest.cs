using System;
using System.Collections.Generic;
using System.Text;

namespace GnomeParty.Models;

public class CombatRequest
{
    public string GameSessionId { get; set; }
    public string EncounterId { get; set; }
    public string TargetCharacterId { get; set; }
    public string SourceCharacterId { get; set; }
    public string Action { get; set; }

    public CombatRequest(string gameSessionId, string encounterId, string targetCharacterId, string sourceCharacterId, string action)
    {
        GameSessionId = gameSessionId;
        EncounterId = encounterId;
        TargetCharacterId = targetCharacterId;
        SourceCharacterId = sourceCharacterId;
        Action = action;
    }
        
    public CombatRequest() : this("", "", "", "", "") { }    

    public CombatRequest DeepCopy()
    {
        return new CombatRequest(GameSessionId,EncounterId, TargetCharacterId, SourceCharacterId, Action);
    }
}
