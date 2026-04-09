using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.BoosPoolActions.GnomeEaterActions
{
    // Primal Roar: Lower the target's attack power by 25% for a turn
    public sealed class PrimalRoar : CharacterAction
    {
        public PrimalRoar() : base("Primal Roar")
        {
            ActionDescription = new CharacterActionDescription("Primal Roar", "Weaken the entire enemy team");
        }
        public override AttackResolution ResolveAttack(
            Character user, 
            Character target, 
            CombatEncounterGameState gameState,
            bool isRedirected = false,
            bool isUnblockable = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user)); // Validate that the user character is not null
            if (target == null) throw new ArgumentNullException(nameof(target)); // Validate that the target character is not null
            if (gameState == null) throw new ArgumentNullException(nameof(gameState)); // Validate that the gameState is not null
            
            var resolution = new AttackResolution(); // Create a variable to hold the atack resolution
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState); // Validate that the targets are eligible for this attack

            // Initialize a list to hold the targets of Primal Roar
            List<Character> primalRoarTargets = TargetingService.GetOpposingTeam(gameState, user.Id);

            // Iterate through each target and create a StausEffectsToApply Instance
            foreach(var currentTarget in primalRoarTargets)
            {
                if (!eligibleTargets.Any(c => c.Id == currentTarget.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(currentTarget)); }
                {
                    resolution.StatusEffectsToApply.Add(new FearStatus(user, target));
                    resolution.Events.Add(new CombatEvent("fear_status_applied", new StatusAppliedEventParams
                    {
                        OwnerId = target.Id,

                    }));
                }
            }
            return resolution;
        }
    }
}
