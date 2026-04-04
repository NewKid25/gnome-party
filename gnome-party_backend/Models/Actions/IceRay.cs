using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions
{
    public class IceRay : CharacterAction
    {
        public IceRay() : base("Ice Ray")
        {
            ActionDescription = new CharacterActionDescription("Ice Ray", "Deal 5 damage to a target and reduce their attack power.");
        }
        public override AttackResolution ResolveAttack(Character user, Character target, CombatEncounterGameState gameState, bool isRedirected = false, bool isUnblockable = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (target == null) throw new ArgumentNullException(nameof(target));
            var resolution = new AttackResolution();
            resolution.AttackInstances = new List<AttackInstance>
            {
                new AttackInstance
                {
                    ActionName = AttackName,
                    BaseDamage = 5,
                    FinalDamage = 5,
                    SourceCharacterId = user.Id,
                    TargetCharacterId = target.Id,
                }
            };
            resolution.StatusEffectsToApply.Add(new ChillStatus(user, target));
            resolution.Events.Add(new CombatEvent("chiil_status_applied", new StatusAppliedEventParams
            {
                OwnerId = user.Id,
            }));

            return resolution;
        }
    }
}
