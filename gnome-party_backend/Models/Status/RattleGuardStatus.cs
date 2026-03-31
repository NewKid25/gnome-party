using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;

namespace Models.Status
{
    public sealed class RattleGuardStatus : StatusEffect
    {
        public RattleGuardStatus() { }
        public RattleGuardStatus(Character user)
        {
            StatusType = StatusTypes.RattleGuard;
            SourceCharacterId = user.Id;
            StatusOwnerCharacterId = user.Id;
            Duration = 1;
            DurationUnit = DurationUnit.TurnStart;
            ModifierValues = new Dictionary<string, double>
            {
                { StatusModifierKeys.DamageReduction, 0.5 }
            };
            StatusDescription = new Dictionary<string, string>
            {
                ["AppliedText"] = $"{user.Name} used Rattle Guard.",
                ["ActiveText"] = $"{user.Name} has Rattle Guard active.",
                ["ExpiredText"] = $"{user.Name} is no longer using Rattle Guard."
            };
        }
        public override StatusEffect DeepCopy()
        {
            return new RattleGuardStatus
            {
                StatusId = StatusId,
                StatusType = StatusType,
                SourceCharacterId = SourceCharacterId,
                StatusOwnerCharacterId = StatusOwnerCharacterId,
                Duration = Duration,
                DurationUnit = DurationUnit,
                ModifierValues = new Dictionary<string, double>(ModifierValues),
                StatusDescription = new Dictionary<string, string>(StatusDescription)
            };
        }
    }
}
