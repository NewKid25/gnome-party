using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions
{
    public sealed class Parry : CharacterAction
    {
        public Parry() : base("Parry")
        {
            ActionDescription = new CharacterActionDescription(
                "Parry",
                "Target an enemy. Take no damage from that enemy this turn."
            );
        }
        public override AttackResolution ResolveAttack(
            Character user,
            Character target,
            CombatEncounterGameState gameState,
            bool isRedirected = false,
            bool isUnblockable = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (target == null) throw new ArgumentNullException(nameof(target));
            var resolution = new AttackResolution();
            resolution.StatusEffectsToApply.Add(new ParryStatus(user, target));
            resolution.Events.Add(new CombatEvent("parry_status_applied", new StatusAppliedEventParams
            {
                OwnerId = user.Id
            }));
            return resolution;
        }
    }
}