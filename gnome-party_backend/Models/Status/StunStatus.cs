using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;

namespace Models.Status
{
    public sealed class StunStatus : StatusEffect
    {
        public StunStatus() { }
        public StunStatus(Character target)
        {
            SourceCharacterId = target.Id;
            StatusOwnerCharacterId = target.Id;
            Duration = 1;
            DurationUnit = DurationUnit.TurnStart;
            StatusDescription = new Dictionary<string, string>
            {
                ["AppliedText"] = $"{target.Name} is now stunned.",
                ["ActiveText"] = $"{target.Name} is stunned and cannot act.",
                ["ExpiredText"] = $"{target.Name} is no longer stunned."
            };
        }

        // Make a deep copy of the status effect
        public override StatusEffect DeepCopy()
        {
            return new StunStatus
            {
                SourceCharacterId = SourceCharacterId,
                StatusOwnerCharacterId = StatusOwnerCharacterId,
                Duration = Duration,
                DurationUnit = DurationUnit,
                AffectedCharacterIds = new List<string>(AffectedCharacterIds),
                ModifierValues = new Dictionary<string, double>(ModifierValues),
                StatusDescription = new Dictionary<string, string>(StatusDescription)
            };
        }
    }
}
