using System;
using System.Collections.Generic;
using System.Text;

namespace Models.ActionMetaData
{
    public class AttackInstance
    {
        public string ActionName { get; set; } = "";
        public int BaseDamage { get; set; }
        public int FinalDamage { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsRedirected { get; set; }
        public string SourceCharacterId { get; set; } = "";
        public string TargetCharacterId { get; set; } = "";
        public bool IsUnblockable { get; set; }
    }
}
