using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class DamageEventParams
    {
        public string SourceId { get; set; }
        public string TargetId { get; set; }
        public string TargetName { get; set; }
        public int DamageAmount { get; set; }
    }
    public class DefeatedEventParams
    {
        public string TargetId { get; set; }
        public string TargetName { get; set; }
    }
}
