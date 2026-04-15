using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.EasyEnemyPoolActions.GoblinArcherActions
{
    // Crippling Shot: Deals 4 damage and reduces the target's damage for 1 turn
    public sealed class CripplingShot : CharacterAction
    {
        public CripplingShot() : base("Crippling Shot")
        {
            ActionDescription = new CharacterActionDescription("Crippling Shot", "Deals 4 damage to a target and reduces the target's attack power");
        }

        public override AttackResolution ResolveAttack(
            Character user, 
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected, 
            bool isUnblockable)
        {
            // Add validation to ensure that the user, target, and gameState are not null
            if (user == null) { throw new ArgumentNullException(nameof(user)); }
            if (target == null) { throw new ArgumentNullException(nameof(target)); }
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            var resolution = new AttackResolution(); // Creare a new attack resolution to hold the results of the attack

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Any(c => c.Id == target.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); }

            // Create a new AttackInstance for the Mockery attack and add it to the resolution
            resolution.AttackInstances = new List<AttackInstance>
            {
                new AttackInstance
                {
                    ActionName = AttackName,
                    BaseDamage = 4,
                    FinalDamage = 4,
                    SourceCharacterId = user.Id,
                    TargetCharacterId = target.Id,
                }
            };

            // Apply the mocked status effect to the target
            resolution.StatusEffectsToApply.Add(new WeakenedStatus(user, target));
            resolution.Events.Add(new CombatEvent("weakened_status_applied", new StatusAppliedEventParams { OwnerId = user.Id }));

            return resolution;
        }
    }
}
