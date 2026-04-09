using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Actions.DifficultEnemyPoolActions.CaveBatActions
{
    // Blood Peck: Deal 5 damage to a target with a chance to heal the user
    public sealed class BloodPeck : CharacterAction
    {
        public BloodPeck() : base("Blood Peck") // Call the base constructor with the action name
        {
            ActionDescription = new CharacterActionDescription("Blood Peck", "Deal 5 damage to a target with a chance to heal the user");
        }

        public override AttackResolution ResolveAttack(Character user, Character target, CombatEncounterGameState gameState, bool isRedirected = false, bool isUnblockable = false)
        {
            // Add validation to ensure that the user, target, and gameState are not null
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Any(c => c.Id == target.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); }

            var resolution = new AttackResolution(); // Create a new AttackResolution object to hold the results of the attack

            resolution.AttackInstances.Add(new AttackInstance 
            {
                ActionName = AttackName,
                BaseDamage = 5,
                FinalDamage = 5,
                SourceCharacterId = user.Id,
                TargetCharacterId = target.Id,
            });

            // Determine if the target's health is 30% or below, and heal the user if that's the case
            double targetHealthPercentage = (double)target.Health / Math.Max(1,target.MaxHealth);
            if (targetHealthPercentage <= 0.3)
            {
                resolution.HealInstances.Add(new HealInstance 
                {
                    SourceCharacterId = target.Id,
                    TargetCharacterId = user.Id,
                    ActionName = AttackName,
                    BaseHealing = 3,
                    FinalHealing = 3,
                });
            }

            return resolution;
        }
    }
}
