using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Status
{
    public sealed class BlockStatus : StatusEffect
    {
        // Default constructor for serialization
        public BlockStatus() { }

        // Constructor for the Block Status Effect
        public BlockStatus(Character user, Character ally)
        {
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

        // Make a deep copy of the status effect
        public override StatusEffect DeepCopy()
        {
            return new BlockStatus
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

        // Modify the damage reduction to reflect the affect of Block Status
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

        // Modify the attack target to reflect the affect of Block Status
        public override Character ModifyRedirectTarget(
            Character source,
            Character originalTarget,
            Character currentTarget,
            CombatEncounterGameState gameState,
            bool isUnblockable,
            bool isUnRedirectable)
        {
            if (isUnblockable || isUnRedirectable) { return currentTarget; }
            if (AffectedCharacterIds.Contains(originalTarget.Id)) // Check to see if the original target is being protected by Block
            {
                // If the block status is active, and the character being attacked is the one being blocked, redirect to blocker
                // Check to make sure the blocker is still alive
                var owner = gameState.PlayerCharacters.Concat(gameState.EnemyCharacters)
                    .FirstOrDefault(c => c.Id == StatusOwnerCharacterId && c.Health > 0);

                if (owner != null)
                {
                    return owner;
                }
            }

            return currentTarget;
        }
    }
}
