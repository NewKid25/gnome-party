using System;
using System.Collections.Generic;
using System.Text;

namespace Models.ActionMetaData
{
    // Data Transfer Object for Healing Actions
    public class HealInstance
    {
        public string SourceCharacterId { get; set; } = "";
        public string TargetCharacterId { get; set; } = "";
        public string ActionName { get; set; } = "";
        public int BaseHealing { get; set; }
        public int FinalHealing { get; set; }
    }
}
