using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;

namespace Models.Status
{
    public sealed class ChillStatus : StatusEffect
    {
        public ChillStatus() { }
        public ChillStatus(Character user, Character enemy)
        {
            AffectedCharacterIds = new List<string> { enemy.Id };
            Duration = 1;
            DurationUnit = DurationUnit.TurnStart;
            SourceCharacterId = user.Id;
            StatusDescription = new Dictionary<string, string>
            {
                ["AppliedText"] = $"{user.Name} applies chill to {enemy.Name}.",
                ["ActiveText"] = $"{enemy.Name}'s attacks are reduced because of the cold.",
                ["ExpiredText"] = $"{enemy.Name} is no longer chilled."
            };
            StatusOwnerCharacterId = user.Id;
        }
        public override StatusEffect DeepCopy()
        {
            return new ChillStatus
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
        public override double ModifyOutgoingDamageMultiplier(Character source, Character target, double currentMultiplier, bool isUnblockable)
        {
            if (AffectedCharacterIds.Contains(source.Id))
            {
                return currentMultiplier * 0.5;
            }
            return currentMultiplier;
        }
    }
}
