using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Actions.EasyEnemyPoolActions.GoblinArcherActions
{
    // Piercing Arrow: Deals 8 damage to a target and cannot be redirected by block
    public sealed class PiercingArrow : CharacterAction
    {
        public PiercingArrow() : base("Piercing Arrow", false, true)
        {
            ActionDescription = new CharacterActionDescription("Piercing Arrow", "Deals 8 damage to a target that can't be redirected");
        }

        public override AttackResolution ResolveAttack(
            Character user, 
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected, 
            bool isUnblockable)
        {
            // Add validation to ensure that the user, target, and gameState are not null
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Any(c => c.Id == target.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); }

            return new AttackResolution // Return a new AttackResolution object with the details of the attack
            {
                AttackInstances = new List<AttackInstance>
                {
                    new AttackInstance
                    {
                        ActionName = AttackName,
                        BaseDamage = 8,
                        FinalDamage = 8,
                        SourceCharacterId = user.Id,
                        TargetCharacterId = target.Id,
                        IsUnblockable = false,
                        IsUnRedirectable = true,
                    }
                }
            };
        }
    }
}
