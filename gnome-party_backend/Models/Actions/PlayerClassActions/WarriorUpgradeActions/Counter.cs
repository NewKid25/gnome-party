using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.PlayerClassActions.WarriorUpgradeActions
{
    // Counter: Target an enemy to take no damage from and attack if targeted by
    public sealed class Counter : CharacterAction
    {
        public Counter() : base("Counter")
        {
            ActionDescription = new CharacterActionDescription("Counter", "Target an enemy. Retaliate against their attack if targeted");
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

            var resolution = new AttackResolution(); // Create a new AttackResolution object
            resolution.StatusEffectsToApply.Add(new CounterStatus(user, target)); // Add a new ParryStatus to the StatusEffectsToApply list
            resolution.Events.Add(new CombatEvent("counter_status_applied", new StatusAppliedEventParams // Add a new CombatEvent to the Events list
            {
                OwnerId = user.Id
            }));
            return resolution;
        }
    }
}
