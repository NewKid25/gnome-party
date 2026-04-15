using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;
using Models.CombatData;
using Models.TestHelperData;

namespace Models.AI.EasyEnemyPoolAI
{
    internal class ForestSpriteAI : CharacterAI
    {
        public ForestSpriteAI() { }
        public ForestSpriteAI(IRandomGenerator rng) : base(rng) { }
        public override CombatRequest ChooseAction(Character self, List<string> actions, List<Character> enemies, List<Character> allies, List<CombatRequest> playerRequests)
        {
            // Defensive check to ensure we have a reference to ourself
            if (self == null) { throw new ArgumentException("Reference to self cannot be null"); }

            // Defensive check to ensure we have actions to choose from
            if (actions == null || actions.Count == 0) { throw new ArgumentException("Actions list cannot be null or empty."); }

            // Defensive check to ensure we have enemies to target
            if (enemies == null || enemies.Count == 0) { throw new ArgumentException("Enemies list cannot be null or empty."); }

            var aliveEnemies = enemies.Where(e => e.Health > 0).ToList(); // Filter out dead enemies
            // If there are no alive enemies, we can't target anyone, so we should handle this case appropriately
            if (aliveEnemies.Count == 0) { throw new InvalidOperationException("No alive enemies to target."); }

            // Initialize decision making variables for entangle
            double entangleChance = 0.6;
            double entangleRoll = Rng.NextDouble();

            // Check if Forest Sprite has Entangle and Leaf Dart
            bool hasEntangle = actions.Contains("Entangle");
            bool hasLeafDart = actions.Contains("Leaf Dart");

            // Check if any "High Impact" actions have been sent to the given Combat Request
            bool highImpactActions = IsHighImpactAction(playerRequests);

            // Target variable if leaf dart is selected
            var leafDartTarget = GetLowestHealthTarget(aliveEnemies);

            // Enable use of Entangle if available to the Forest Sprite, successful Entangle Roll, and High Impact move was used by an enemy
            if (hasEntangle && entangleRoll <= entangleChance && highImpactActions)
            {
                Character entangleTarget = GetHighImpactUser(playerRequests, aliveEnemies);
                return new CombatRequest
                {
                    Action = "Entangle",
                    TargetCharacterId = entangleTarget.Id
                };
            }
            else if(hasLeafDart) { return new CombatRequest { Action = "Leaf Dart", TargetCharacterId = leafDartTarget.Id }; }
            else { return new CombatRequest { Action = GetDefaultAction(actions), TargetCharacterId = GetLowestHealthTarget(aliveEnemies).Id }; }
        }
        public override CombatRequest ChooseAction(Character self, List<string> actions, List<Character> enemies, List<Character> allies)
        {
            return new CombatRequest
            {
                Action = GetDefaultAction(actions),
                TargetCharacterId = GetLowestHealthTarget(enemies).Id
            };
        }
    }
}
