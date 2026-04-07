using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Actions.WarriorActions
{
    // Whirling Strike: Deal 5 damage to all enemies.
    public sealed class WhirlingStrike : CharacterAction
    {
        public WhirlingStrike() : base("Whirling Strike") // Call the base constructor with the name of the action
        {
            ActionDescription = new CharacterActionDescription("Whirling Strike", "Deal 5 damage to all enemies"); // Set the action description
        }

        // Override the ResolveAttack method to implement the logic for dealing damage to all enemies
        public override AttackResolution ResolveAttack(
            Character user, 
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected = false, 
            bool isUnblockable = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user)); // Validate that the user is not null
            if (target == null) throw new ArgumentNullException(nameof(target)); // Validate that the target is not null
            List<Character> whirlStrikeTargets; // Initialize a list to hold the targets of the Whirling Strike
            if (isRedirected) // If the attack is redirected, only target the specified target
            {
                whirlStrikeTargets = new List<Character> { target };
            }
            else // If the attack is not redirected, target all opposing team members
            {
                whirlStrikeTargets = TargetingService.GetOpposingTeam(gameState, user.Id);
            }

            var resolution = new AttackResolution(); // Create a new AttackResolution object to hold the results of the attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState); // Validate that the targets are eligible for this attack

            foreach (var currentTarget in whirlStrikeTargets) // Iterate through each target and create an AttackInstance for each one
            {
                // validate that the target is eligible for this attack
                if (!isRedirected && !eligibleTargets.Any(c => c.Id == currentTarget.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); } 

                resolution.AttackInstances.Add(new AttackInstance
                {
                    ActionName = AttackName,
                    BaseDamage = 5,
                    FinalDamage = 5,
                    IsRedirected = isRedirected,
                    SourceCharacterId = user.Id,
                    TargetCharacterId = currentTarget.Id,
                });
            }
            return resolution;
        }
    }
}
