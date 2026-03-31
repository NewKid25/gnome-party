using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;

namespace Models.Status
{
    public sealed class BlockStatus : StatusEffect
    {
       public BlockStatus() { }
        public BlockStatus(Character user, Character ally)
        {
            StatusType = StatusTypes.Block;
            SourceCharacterId = user.Id;
            StatusOwnerCharacterId = user.Id;
            Duration = 1;
            DurationUnit = DurationUnit.TurnStart;
            AffectedCharacterIds.Add(ally.Id);
            ModifierValues = new Dictionary<string, double>
            {
                { StatusModifierKeys.DamageReduction, 0.5 }
            };
            StatusDescription = new Dictionary<string, string>
            {
                ["AppliedText"] = $"{user.Name} is guarding {ally.Name}.",
                ["ActiveText"] = $"{ally.Name} is being guarded by {user.Name}.",
                ["ExpiredText"] = $"{user.Name} is no longer guarding {ally.Name}."
            };
        }
        public override StatusEffect DeepCopy()
        {
            return new BlockStatus
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
