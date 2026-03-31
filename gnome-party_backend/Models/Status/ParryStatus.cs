using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;

namespace Models.Status
{
    public sealed class ParryStatus : StatusEffect
    {
        public ParryStatus() { }
        public ParryStatus(Character user, Character enemy)
        {
            StatusType = StatusTypes.Parry;
            SourceCharacterId = user.Id;
            StatusOwnerCharacterId = user.Id;
            Duration = 1;
            DurationUnit = DurationUnit.TurnStart;
            AffectedCharacterIds = new List<string> { enemy.Id };
            StatusDescription = new Dictionary<string, string>
            {
                ["AppliedText"] = $"{user.Name} activates parry against {enemy.Name}.",
                ["ActiveText"] = $"{enemy.Name}'s attack is parried by {user.Name}",
                ["ExpiredText"] = $"{user.Name} is no longer parrying {enemy.Name}."
            };
        }
        public override StatusEffect DeepCopy()
        {
            return new ParryStatus
            {
                StatusId = StatusId,
                StatusType = StatusType,
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
