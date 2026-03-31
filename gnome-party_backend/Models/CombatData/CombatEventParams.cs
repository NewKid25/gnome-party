using System;
using System.Collections.Generic;
using System.Text;

namespace Models.CombatData
{
    public class DamageEventParams
    {
        public int DamageAmount { get; set; }
        public string SourceId { get; set; }
        public string TargetId { get; set; }
        public string TargetName { get; set; }
    }
    public class DefeatedEventParams
    {
        public string TargetId { get; set; }
        public string TargetName { get; set; }
    }
}
