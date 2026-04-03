using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions
{
    public sealed class Fireball : CharacterAction
    {
        public Fireball() : base("Fireball")
        {
            ActionDescription = new CharacterActionDescription("Fireball", "Deal damage to the target and then burn the target and adjacent allies for 3 turns");
        }
        public override AttackResolution ResolveAttack(
            Character user, 
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected = false, 
            bool isUnblockable = false)
        {
            const int burnDuration = 3;
            const int burnTickDamage = 2;
            if (user == null) throw new ArgumentNullException(nameof(user));
            if(target == null) throw new ArgumentNullException(nameof(target));
            if(gameState == null) throw new ArgumentNullException(nameof(gameState));
            var resolution = new AttackResolution();
            resolution.AttackInstances.Add(new AttackInstance
            {
                ActionName = AttackName,
                BaseDamage = 6,
                FinalDamage = 6,
                IsRedirected = isRedirected,
                SourceCharacterId = user.Id,
                TargetCharacterId = target.Id,
            });
            List<Character> burnTargeets;
            if (isRedirected)
            {
                burnTargeets = new List<Character> { target };
            }
            else
            {
                burnTargeets = TargetingService.GetTargetAndAdjacentAllies(gameState, target.Id);
            }
            foreach(var burnTarget in burnTargeets)
            {
                resolution.StatusEffectsToApply.Add(new BurnStatus(user, burnTarget, burnDuration, burnTickDamage));
                resolution.Events.Add(new CombatEvent("burn_status_applied", new StatusAppliedEventParams
                {
                    OwnerId = burnTarget.Id,
                }));
            }
            return resolution;
        }
    }
}
