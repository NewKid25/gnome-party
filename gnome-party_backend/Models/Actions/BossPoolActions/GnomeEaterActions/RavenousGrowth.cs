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
            if (user == null) throw new ArgumentNullException(nameof(user)); // Null check the user
            if (gameState == null) throw new ArgumentNullException(nameof(gameState)); // Null check the gamestate

            // Check if the user is a Gnome Eater
            if(user is not GnomeEater gnomeEater) { throw new InvalidOperationException("Ravenous Growth can only be used by the Gnome Eater"); }

            gnomeEater.PermaDamageBoost += 2; // Apply permanent damage buff

            // Return an attack resolution
            var resolution = new AttackResolution();
            resolution.Events.Add(new CombatEvent("ravenous_growth_applied", new
            {
                sourceId = user.Id,
                newDamageBoost = gnomeEater.PermaDamageBoost,
            }));

            return resolution;
        }
    }
}
