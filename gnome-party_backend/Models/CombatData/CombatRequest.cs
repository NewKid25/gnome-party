using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Swift;
using System.Text;

namespace Models.CombatData;

public class CombatRequest
{
    public string Action { get; set; }
    public string EncounterId { get; set; }
    public string GameSessionId { get; set; }
    public string SourceCharacterId { get; set; }
    public string TargetCharacterId { get; set; }
    public List<string> TargetCharacterIds { get; set; } = new();
    public CombatRequest() : this("", "", "", "", "") { }    
    public CombatRequest(string gameSessionId, string encounterId, string targetCharacterId, string sourceCharacterId, string action)
    {
        GameSessionId = gameSessionId;
        EncounterId = encounterId;
        TargetCharacterId = targetCharacterId;
        SourceCharacterId = sourceCharacterId;
        Action = action;

        if (!string.IsNullOrWhiteSpace(targetCharacterId)) { TargetCharacterIds = new List<string> { targetCharacterId }; }
    }
    public CombatRequest DeepCopy()
    {
        return new CombatRequest(GameSessionId, EncounterId, TargetCharacterId, SourceCharacterId, Action)
        { TargetCharacterIds = new List<string>(TargetCharacterIds) };
    }
}
