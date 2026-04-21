using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Actions.BossPoolActions.NecrognomancerActions
{
    // Summon: Summon an allied monster (Weakened Skeleton right now)
    public sealed class Summon : CharacterAction
    {
        public Summon() : base("Summon") // Call the base constructor with the name of the action
        {
            ActionDescription = new CharacterActionDescription("Summon", "Summon a weakened monster as an ally"); // Set the action description
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

            var resolution = new AttackResolution(); // Create a new AttackResolution object to hold the results of the attack

            int summonCount = 0;
            foreach(var character in gameState.EnemyCharacters)
            {
                if(character.CharacterType == "Summon")
                {
                    summonCount++;
                }
            }

            return resolution;
        }
    }
}
