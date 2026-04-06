using System;
using System.Collections.Generic;
using System.Text;

namespace Models.ActionMetaData
{
    // Details about a target for an action
    public class TargetInfo
    {
        public string CharacterId { get; set; } = "";
        public string Name { get; set; } = "";
        public string CharacterType { get; set; } = "";
        public int Health { get; set; }
        public int MaxHealth { get; set; }
    }
}