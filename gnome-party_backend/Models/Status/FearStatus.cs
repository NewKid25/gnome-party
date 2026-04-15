using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;

namespace Models.Status
{
    public sealed class FearStatus : StatusEffect
    {
        public FearStatus() { }
        public FearStatus(Character source, Character target)
        {
            SourceCharacterId = source.Id;
            StatusOwnerCharacterId = target.Id;
            Duration = 1;
            DurationUnit = DurationUnit.TurnStart;

            ModifierValues = new Dictionary<string, double>
            {
                [StatusModifierKeys.OutgoingDamageMultiplier] = 0.75 // Reduce damage by 25% (75% % attack power)
            };

            StatusDescription = new Dictionary<string, string>
            {
                ["AppliedText"] = $"{target.Name} is now fearful.",
                ["ActiveText"] = $"{target.Name}'s attacks are weaker.",
                ["ExpiredText"] = $"{target.Name} is no longer fearful."
            };
        }
        // Make a deep copy of the status effect
        public override StatusEffect DeepCopy()
        {
            return new FearStatus
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
