using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;

namespace Models.Status
{
    public sealed class RattleGuardStatus : StatusEffect
    {
        public RattleGuardStatus() { } // Parameterless constructor for deserialization

        // Constructor to initialize the Rattle Guard status with the user character
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
            SourceCharacterId = user.Id;
        }

        // Creates a deep copy of the RattleGuardStatus instance
        public override StatusEffect DeepCopy()
        {
            return new RattleGuardStatus
            {
                Duration = Duration,
                DurationUnit = DurationUnit,
                ModifierValues = new Dictionary<string, double>(ModifierValues),
                StatusDescription = new Dictionary<string, string>(StatusDescription),
                StatusOwnerCharacterId = StatusOwnerCharacterId,
                SourceCharacterId = SourceCharacterId,
            };
        }

        // Modifies the damage reduction to reflect the affect of Rattle Guard
        public override double ModifyDamageReduction(
            Character source,
            Character target,
            double currentReduction,
            bool isUnblockable)
        {
            if (isUnblockable)
            {
                return currentReduction;
            }

            if (ModifierValues.TryGetValue(StatusModifierKeys.DamageReduction, out var value))
            {
                return currentReduction + value;
            }

            return currentReduction;
        }
    }
}
