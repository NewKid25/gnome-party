using System;
using System.Collections.Generic;
using System.Text;

namespace Models.ActionMetaData
{
    public class ActionTargetInfo
    {
        public string ActionName { get; set; } = "";
        public List<TargetInfo> EligibleTarget { get; set; } = new();
    }
}
