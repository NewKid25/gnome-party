using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using Models.CharacterData;
using Models.CombatData;
using Models.TestHelperData;

namespace Models.AI.DifficultEnemyPoolAI
{
    internal class CaveBatAI : CharacterAI
    {
        public CaveBatAI() { }
        public CaveBatAI(IRandomGenerator rng) : base(rng) { }
        public override CombatRequest ChooseAction(Character self, List<string> actions, List<Character> enemies, List<Character> allies)
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

            // Verify if all the Cave Bat Actions are present
            bool hasBloodPeck = actions.Contains("Blood Peck");
            bool hasSonicSqueal = actions.Contains("Sonic Squeal");

            string chosenAction = null;

            bool enemiesWithLowHealth = false;

            // Check if any target's health is 30% or less
            double lowHealthMarker = 0.3;
            foreach(var enemy in aliveEnemies)
            {
                double healthPercentage = (double)enemy.Health / Math.Max(1, enemy.MaxHealth);
                if(healthPercentage <= lowHealthMarker)
                {
                    enemiesWithLowHealth = true;
                    break;
                }
            }

            // If at least one target has 30% health or less, 75% chance to use Blood Peck on the target with the least health
            // Otherwise use Sonic Squeal
            double bloodPeckChance = 0.75;
            if (hasBloodPeck && enemiesWithLowHealth && Rng.NextDouble() <= bloodPeckChance) { chosenAction = "Blood Peck"; }
            else if (hasSonicSqueal)  { chosenAction = "Sonic Squeal"; }
            else  { chosenAction = GetDefaultAction(actions); }

            var target = GetLowestHealthTarget(aliveEnemies); // Call a help method to target the enemy with the lowest health

            return new CombatRequest { Action = chosenAction, TargetCharacterId = target.Id }; // Create and return a CombatRequest with the chosen action and target
        }
    }
}
