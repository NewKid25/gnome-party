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
            StatusOwnerCharacterId = user.Id;
            StatusType = StatusTypes.RattleGuard;
            SourceCharacterId = user.Id;
        }
        public override StatusEffect DeepCopy()
        {
            return new RattleGuardStatus
            {
                Duration = Duration,
                DurationUnit = DurationUnit,
                ModifierValues = new Dictionary<string, double>(ModifierValues),
                StatusDescription = new Dictionary<string, string>(StatusDescription),
                StatusId = StatusId,
                StatusOwnerCharacterId = StatusOwnerCharacterId,
                StatusType = StatusType,
                SourceCharacterId = SourceCharacterId,
            };
        }
    }
}
