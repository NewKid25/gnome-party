using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Actions.BoosPoolActions.GnomeEaterActions
{
    // Devour Essence: Deal 8 damage and then heal the user for 8 health
    public sealed class DevourEssence : CharacterAction
    {
        public DevourEssence() : base("Devour Essence")
        {
            ActionDescription = new CharacterActionDescription("Devour Essence", "Deals 8 damage to a target and heals user for 8 health");
        }

        public override AttackResolution ResolveAttack(
            Character user, 
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected = false, 
            bool isUnblockable = false)
        {
            // Add validation to ensure that the user, target, and gameState are not null
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Any(c => c.Id == target.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); }

            var resolution = new AttackResolution(); // Create a new AttackResolution object to hold the results of the attack

            // Deal the 8 damage attack
            resolution.AttackInstances.Add(new AttackInstance
            {
                ActionName = AttackName,
                BaseDamage = 8,
                FinalDamage = 8,
                SourceCharacterId = user.Id,
                TargetCharacterId = target.Id,
            });

            // Heal for 8 health
            resolution.HealInstances.Add(new HealInstance
            {
                SourceCharacterId = target.Id,
                TargetCharacterId = user.Id,
                ActionName = AttackName,
                BaseHealing = 8,
                FinalHealing = 8,
            });

            return resolution;
        }
    }
}
