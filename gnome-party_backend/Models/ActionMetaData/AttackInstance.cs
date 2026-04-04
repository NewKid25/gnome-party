using System;
using System.Collections.Generic;
using System.Text;

namespace Models.ActionMetaData
{
    // This class represents the result of a single attack instance, which may be part of a larger attack resolution
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
