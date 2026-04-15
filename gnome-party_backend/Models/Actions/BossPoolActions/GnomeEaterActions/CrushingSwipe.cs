using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CharacterData.BossEnemyPoolClasses;
using Models.CombatData;

namespace Models.Actions.BoosPoolActions.GnomeEaterActions
{
    // Crushing Swipe: Deal 14 damage to a single target
    public sealed class CrushingSwipe : CharacterAction
    {
        public CrushingSwipe() : base("Crushing Swipe")
        {
            ActionDescription = new CharacterActionDescription("Crushing Swipe", "Deal 14 damage to a target enemy");
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

            int permBoost = 0;
            if(user is GnomeEater gnomeEater) { permBoost = gnomeEater.PermaDamageBoost; }

            return new AttackResolution // Return an AttackResolution object that describes the result of the Slash action
            {
                AttackInstances = new List<AttackInstance>
                {
                    new AttackInstance
                    {
                        ActionName = AttackName,
                        BaseDamage = 14 + permBoost, 
                        FinalDamage = 14 + permBoost,
                        SourceCharacterId = user.Id,
                        TargetCharacterId = target.Id,
                    }
                }
            };
        }
    }
}
