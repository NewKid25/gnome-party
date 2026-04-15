using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;

namespace Models.Status
{
    public sealed  class VulnerableStatus : StatusEffect
    {
        public VulnerableStatus() { } 
        public VulnerableStatus(Character user, Character target)
        {
            Duration = 2; // Lasts for 1 turn
            DurationUnit = DurationUnit.TurnEnd; // Expires at the end of the turn
            SourceCharacterId = user.Id; // The character who applied the status
            StatusDescription = new Dictionary<string, string> // Descriptions for different stages of the status
            {
                ["AppliedText"] = $"{target.Name} is now vulnerable.",
                ["ActiveText"] = $"{target.Name} will receive extra damage.",
                ["ExpiredText"] = $"{target.Name} is no longer vulnerable."
            };
            StatusOwnerCharacterId = target.Id; // The character affected by the status
            AffectedCharacterIds = new List<string> { target.Id }; // The character whose attacks will be reduced
        }
        // Make a deep copy of the status effect
        public override StatusEffect DeepCopy()
        {
            return new VulnerableStatus
            {
                AffectedCharacterIds = new List<string>(AffectedCharacterIds),
                Duration = Duration,
                DurationUnit = DurationUnit,
                ModifierValues = new Dictionary<string, double>(ModifierValues),
                SourceCharacterId = SourceCharacterId,
                StatusDescription = new Dictionary<string, string>(StatusDescription),
                StatusOwnerCharacterId = StatusOwnerCharacterId,
            };
        }
    }
}
