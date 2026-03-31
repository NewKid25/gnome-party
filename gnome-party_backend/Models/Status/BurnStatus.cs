using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;

namespace Models.Status
{
    public sealed class BurnStatus : StatusEffect
    {
        public BurnStatus() { }

        public BurnStatus(Character source, Character target, int duration = 3, int tickDamage = 2)
        {
            StatusType = StatusTypes.Burn;
            SourceCharacterId = source.Id;
            StatusOwnerCharacterId = target.Id;
            Duration = duration;
            DurationUnit = DurationUnit.TurnStart;
            AffectedCharacterIds = new List<string> { target.Id };

            ModifierValues = new Dictionary<string, double>
            {
                [StatusModifierKeys.TickDamage] = tickDamage
            };
        }
        public override StatusEffect DeepCopy()
        {
            return new BurnStatus
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
