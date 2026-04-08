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


            throw new NotImplementedException();
        }
    }
}
