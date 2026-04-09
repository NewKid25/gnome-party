using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Actions.DifficultEnemyPoolActions.CaveBatActions
{
    // Sonic Squeal: Deal 3 damage to all players
    public sealed class SonicSqueal : CharacterAction
    {
        public SonicSqueal() : base("Sonic Squeal") // Call the base constructor with the action name
        {
            ActionDescription = new CharacterActionDescription("Sonic Squeal", "Deal 3 damage to all enemies");
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

            // Initialize a list to hold the targets of Sonic Squeal
            List<Character> sonicSquealTargets = TargetingService.GetOpposingTeam(gameState, user.Id);

            var resolution = new AttackResolution(); // Create a new AttackResolution object to hold the results of the attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState); // Validate that the targets are eligible for this attack

            // Iterate through each target and create an AttackInstance for each one
            foreach(var currentTarget in sonicSquealTargets)
            {
                if(!eligibleTargets.Any(c => c.Id == currentTarget.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(currentTarget)); }

                resolution.AttackInstances.Add(new AttackInstance 
                {
                    ActionName = AttackName,
                    BaseDamage = 3,
                    FinalDamage = 3,
                    IsRedirected = isRedirected,
                    SourceCharacterId = user.Id,
                    TargetCharacterId = currentTarget.Id,
                });
            }
            return resolution;
        }
    }
}
