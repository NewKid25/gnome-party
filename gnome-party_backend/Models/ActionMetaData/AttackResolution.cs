using System;
using System.Collections.Generic;
using System.Text;
using Models.CombatData;
using Models.Status;

namespace Models.ActionMetaData
{
    public class AttackResolution
    {
        public List<AttackInstance> AttackInstances { get; set; } = new();
        public List<StatusEffect> StatusEffectsToApply { get; set; } = new();
        public List<CombatEvent> Events { get; set; } = new();
    }
}
