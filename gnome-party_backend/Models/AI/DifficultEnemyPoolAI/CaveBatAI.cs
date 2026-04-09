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

            if (actions == null || actions.Count == 0) // Defensive check to ensure we have actions to choose from
            {
                throw new ArgumentException("Actions list cannot be null or empty.");
            }
            if (enemies == null || enemies.Count == 0) // Defensive check to ensure we have enemies to target
            {
                throw new ArgumentException("Enemies list cannot be null or empty.");
            }
            var aliveEnemies = enemies.Where(e => e.Health > 0).ToList(); // Filter out dead enemies
            if (aliveEnemies.Count == 0) // If there are no alive enemies, we can't target anyone, so we should handle this case appropriately
            {
                throw new InvalidOperationException("No alive enemies to target.");
            }

            // Verify if all the Cave Bat Actions are present
            bool hasBloodPeck = actions.Contains("Blood Peck");
            bool hasSonicSqueal = actions.Contains("Sonic Squeal");

            string chosenAction = null;

            bool enemiesWithLowHealth = false;

            // Check if any target's health is 30% or less
            foreach(var enemy in aliveEnemies)
            {
                double healthPercentage = (double)enemy.Health / Math.Max(1, enemy.MaxHealth);
                if(healthPercentage <= 0.3)
                {
                    enemiesWithLowHealth = true;
                    break;
                }
            }

            // If at least one target has 30% health or less, 75% chance to use Blood Peck on the target with the least health
            // Otherwise use Sonic Squeal

            if (hasBloodPeck && enemiesWithLowHealth && Rng.NextDouble() <= 0.75)
            {
                chosenAction = "Blood Peck";
            }
            else if (hasSonicSqueal)
            {
                chosenAction = "Sonic Squeal";
            }
            else
            {
                chosenAction = GetDefaultAction(actions);
            }

            // Four step lowest health priority targeting with a random factor for an ultimate tie breaker

            // Check 1: Get the target(s) with the lowest health percentage
            double lowestHealthPercentage = aliveEnemies.Min(e => (double)e.Health / Math.Max(1, e.MaxHealth));
            var lowestPercentTargets = aliveEnemies.Where(e => (double)e.Health / Math.Max(1, e.MaxHealth) == lowestHealthPercentage).ToList();

            // Cjheck 2: Get the target(s) with the lowest overall health
            int lowestHealh = lowestPercentTargets.Min(e => e.Health);
            var lowestHealhTargets = lowestPercentTargets.Where(e => e.Health == lowestHealh).ToList();

            // Check 3: Get the target(s) with the highest max health
            int highestMaxHealth = lowestHealhTargets.Max(e => e.MaxHealth);
            var finalTargets = lowestHealhTargets.Where(e => e.MaxHealth == highestMaxHealth).ToList();

            // Check 4: Pick a random victim from the final target list
            int index;
            if (finalTargets.Count == 1)
            {
                index = 0;
            }
            else
            {
                index = (int)(Rng.NextDouble() * finalTargets.Count);
                if (index >= finalTargets.Count)
                {
                    index = finalTargets.Count - 1;
                }
            }
            if (finalTargets.Count == 0)
            {
                throw new InvalidOperationException("No valid final target candidates were found.");
            }
            var target = finalTargets[index];

            return new CombatRequest { Action = chosenAction, TargetCharacterId = target.Id }; // Create and return a CombatRequest with the chosen action and target
        }
    }
}
