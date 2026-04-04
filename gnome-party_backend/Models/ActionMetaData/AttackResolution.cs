using System;
using System.Collections.Generic;
using System.Text;
using Models.CombatData;
using Models.Status;

namespace Models.ActionMetaData
{
    // This class represents the overall result of resolving an attack action, which may include multiple attack instances, combat events, and status effects to apply
    public class AttackResolution
    {
        public List<AttackInstance> AttackInstances { get; set; } = new();
        public List<CombatEvent> Events { get; set; } = new();
        public List<StatusEffect> StatusEffectsToApply { get; set; } = new();
    }
}
