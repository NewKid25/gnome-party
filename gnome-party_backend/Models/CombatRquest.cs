using System;
using System.Collections.Generic;
using System.Text;

namespace GnomeParty.Models;

public class CombatRequest
{
        public string EncounterId { get; set; }
        public string TargetCharacterId { get; set; }
        public string SourceCharacterId { get; set; }
        public string Attack { get; set; }

        public CombatRequest(string encounterId, string targetCharacterId, string sourceCharacterId, string attack)
        {
            EncounterId = encounterId;
            TargetCharacterId = targetCharacterId;
            SourceCharacterId = sourceCharacterId;
            Attack = attack;
        }
        
        public CombatRequest() : this("", "", "", "") { }    
}
