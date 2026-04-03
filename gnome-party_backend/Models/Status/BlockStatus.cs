using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Status
{
    public sealed class BlockStatus : StatusEffect
    {
        public BlockStatus() { }
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
        public override Character ModifyRedirectTarget(
            Character source,
            Character originalTarget,
            Character currentTarget,
            CombatEncounterGameState gameState,
            bool isUnblockable)
        {
            if (AffectedCharacterIds.Contains(originalTarget.Id))
            {
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
