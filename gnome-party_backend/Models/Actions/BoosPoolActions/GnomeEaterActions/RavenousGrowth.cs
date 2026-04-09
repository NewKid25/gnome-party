using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CharacterData.BossEnemyPoolClasses;
using Models.CombatData;

namespace Models.Actions.BoosPoolActions.GnomeEaterActions
{
    // Ravenous Growth: Permanently add +2 to the user's attack
    public sealed class RavenousGrowth : CharacterAction
    {
        public RavenousGrowth() : base("Ravenous Growth")
        {
            ActionDescription = (new CharacterActionDescription("Ravenous Growth", "User grows permanently stronger"));
        }
        public override AttackResolution ResolveAttack(
            Character user, 
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected = false, 
            bool isUnblockable = false)
        {

            throw new NotImplementedException();
        }
    }
}
