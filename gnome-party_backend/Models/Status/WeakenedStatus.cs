using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;

namespace Models.Status
{
    public sealed class WeakenedStatus : StatusEffect
    {
        public WeakenedStatus() { }
        public WeakenedStatus(Character user, Character enemy)
        {
            Duration = 1; // Lasts for 1 turn
            DurationUnit = DurationUnit.TurnEnd; // Expires at the end of the turn
            SourceCharacterId = user.Id; // The character who applied the status
            StatusDescription = new Dictionary<string, string> // Descriptions for different stages of the status
            {
                ["AppliedText"] = $"{user.Name}'s attack has weakened {enemy.Name}.",
                ["ActiveText"] = $"{enemy.Name}'s attacks are reduced.",
                ["ExpiredText"] = $"{enemy.Name} is no longer weakened."
            };
            StatusOwnerCharacterId = enemy.Id; // The character affected by the status
            AffectedCharacterIds = new List<string> { enemy.Id }; // The character whose attacks will be reduced
        }
        // Make a deep copy of the status effect
        public override StatusEffect DeepCopy()
        {
            return new WeakenedStatus
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
        // Modify the outgoing damage multiplier to reflect the affect of Chill Status
        public override double ModifyOutgoingDamageMultiplier(Character source, Character target, double currentMultiplier, bool isUnblockable)
        {
            if (AffectedCharacterIds.Contains(source.Id))
            {
                return currentMultiplier * 0.75;
            }
            return currentMultiplier;
        }
    }
}
