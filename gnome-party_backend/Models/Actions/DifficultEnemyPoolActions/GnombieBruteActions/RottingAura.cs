using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.DifficultEnemyPoolActions.GnombieBruteActions
{
    // Rotten Aura: Debuff your target with +2 incoming damage
    public sealed class RottingAura : CharacterAction
    {
        public RottingAura() : base("Rotting Aura") // Call the base constructor with the action name
        {
            ActionDescription = new CharacterActionDescription("Rotting Aura", "Make your target vulnerable and receive extra damage"); // Set the action description
        }
        public override AttackResolution ResolveAttack(
            Character user,
            Character target,
            CombatEncounterGameState gameState,
            bool isRedirected,
            bool isUnblockable)
        {
            if (user == null) throw new ArgumentNullException(nameof(user)); // Validate that the user character is not null
            if (target == null) throw new ArgumentNullException(nameof(target)); // Validate that the target character is not null
            if (gameState == null) throw new ArgumentNullException(nameof(gameState)); // Validate that the gameState is not null

            var resolution = new AttackResolution(); // Creare a new attack resolution to hold the results of the attack

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Any(c => c.Id == target.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); }

            // Apply the Stun Status effect to the target
            resolution.StatusEffectsToApply.Add(new VulnerableStatus(user, target));
            resolution.Events.Add(new CombatEvent("vulnerable_status_applied", new StatusAppliedEventParams { OwnerId = target.Id }));

            return resolution;
        }
    }
}
