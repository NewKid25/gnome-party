using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.ExtraActions
{
    public sealed class StunDefaultAction : CharacterAction
    {
        public StunDefaultAction() : base("Stunned")
        {
            ActionDescription = new CharacterActionDescription("Stunned", "Unable to act while stunned");
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
            
            var resolution = new AttackResolution(); // Create a new AttackResolution object to hold the results of the action
            resolution.StatusEffectsToApply.Add(new StunStatus(user));
            resolution.Events.Add(new CombatEvent("stun_status_applied", new {sourceId = user.Id, ownerId = user.Id, targetId = user.Id }));
            return resolution;
        }
    }
}
