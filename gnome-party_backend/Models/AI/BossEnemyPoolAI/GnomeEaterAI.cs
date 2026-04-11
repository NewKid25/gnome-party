using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CharacterData.BossEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.TestHelperData;

namespace Models.AI.BossEnemyPoolAI
{
    internal class GnomeEaterAI : CharacterAI
    {
        // Calls 3 instances of Rng.
        // 1. 60% chance to Use Devour Essence when having 50% health or less
        // 2. 40% channce to use Primal Roar when half of the enemy (or more) has 55% health or more
        // 3. Priority targeting for each player class:
        //      * 50% chance to target the Warrior Class
        //      * 30% chance to target the Mage Class
        //      * 20% chance to target the Bard Class
        public GnomeEaterAI() { }
        public GnomeEaterAI(IRandomGenerator rng) : base(rng) { }
        public override CombatRequest ChooseAction(Character self, List<string> actions, List<Character> enemies, List<Character> allies)
        {
            // Defensive check to ensure we have a reference to ourself
            if (self == null) { throw new ArgumentException("Reference to self cannot be null");}

            // Defensive check to ensure we have actions to choose from
            if (actions == null || actions.Count == 0){ throw new ArgumentException("Actions list cannot be null or empty.");}

            // Defensive check to ensure we have enemies to target
            if (enemies == null || enemies.Count == 0) { throw new ArgumentException("Enemies list cannot be null or empty.");}

            var aliveEnemies = enemies.Where(e => e.Health > 0).ToList(); // Filter out dead enemies
            // If there are no alive enemies, we can't target anyone, so we should handle this case appropriately
            if (aliveEnemies.Count == 0) { throw new InvalidOperationException("No alive enemies to target.");}

            // Verify if all the Gnome Eater actions are present
            bool hasCrushingSwipe = actions.Contains("Crushing Swipe");
            bool hasDevourEssence = actions.Contains("Devour Essence");
            bool hasPrimalRoar = actions.Contains("Primal Roar");
            bool hasRavenousGrowth = actions.Contains("Ravenous Growth");

            double gnomeEaterHealthPercentage = (double)self.Health / Math.Max(1, self.MaxHealth); // Variable to find what health the Gnome Eater is at

            string chosenAction = null; // Variable to hold the chosen action

            // Variables for using Ravenous Growth
            int ravGrowthRequiredTurnCount = 4;

            // Variables for using Devour Essence
            double devourEssenceChance = 0.6;
            double devEssRequiredHealthPercentage = 0.5;

            // Variables for using Primal Roar
            // Iterate through the alive enemy list and find targets with 60% health or more
            // "Multiple players are alive and healthy" means (to me at least. lol) half (rounded up) of the team has 60% health or more
            double healthyPercentMarker = 0.55;
            int healthyCountDivisor = 2;
            int aliveEnemyCount = aliveEnemies.Count(); // Get the number of alive enemies
            int healthyEnemyCount = aliveEnemies.Count(e => (double)e.Health / Math.Max(1, e.MaxHealth) >= healthyPercentMarker); // Healthy enemies = alive with more than the required health
            bool healthyEnemyTeam = healthyEnemyCount >= (aliveEnemyCount / healthyCountDivisor); // True/False variable for if number of healthy enemies meets the required percentage
            double primalRoarChance = 0.4;

            if (self is not GnomeEater gnomeEater) { throw new ArgumentException("Gnome Eater AI is only valid for the Gnome Eater"); }

            // Guaranteed use of Ravenous Growth every 4th turn 
            if (hasRavenousGrowth && gnomeEater.turnCounter > 0 && (gnomeEater.turnCounter % ravGrowthRequiredTurnCount == 0)) { chosenAction = "Ravenous Growth"; }

            // Choose Devour Essence when health is 50% or less and successful 60% roll
            else if (hasDevourEssence && gnomeEaterHealthPercentage <= devEssRequiredHealthPercentage && Rng.NextDouble() <= devourEssenceChance) { chosenAction = "Devour Essence"; }

            // Choose Primal Roar if half the enemy team has 60%+ health and successful 40% roll
            else if(hasPrimalRoar && healthyEnemyTeam && Rng.NextDouble() <= primalRoarChance) { chosenAction = "Primal Roar"; }

            // Choose Crushing Swipe if it was passed to the Gnome Eater (otherwise call the default action)
            else if (hasCrushingSwipe) { chosenAction = "Crushing Swipe"; }
            else { chosenAction = GetDefaultAction(actions); }

            Character target = null;

            // Variables to decide which player classes have a higher priority chance to be targeted
            double warriorTargetChance = 0.5; 
            double mageTargetChance = 0.3; 
            double bardTargetChance = 0.2; 

            // Separate alive enemies by character class
            List<Character> aliveWarriorTargets = aliveEnemies.Where(e => e is Warrior).ToList();
            List<Character> aliveMageTargets = aliveEnemies.Where(e => e is Mage).ToList();
            List<Character> aliveBardTargets = aliveEnemies.Where(e => e is Bard).ToList();

            // Create weighted/priority target groups for alive instance of player classes
            var weightedGroups = new List<(List<Character> Targets, double Weight)>();
            if (aliveWarriorTargets.Count > 0) { weightedGroups.Add((aliveWarriorTargets, warriorTargetChance)); }
            if (aliveMageTargets.Count > 0) { weightedGroups.Add((aliveMageTargets, mageTargetChance)); }
            if (aliveBardTargets.Count > 0) { weightedGroups.Add((aliveBardTargets, bardTargetChance)); }
            if(weightedGroups.Count == 0) { target = GetLowestHealthTarget(aliveEnemies); }
            else
            {
                double totalWeight = weightedGroups.Sum(g => g.Weight); // Calculate the total priority weight (based on classes left)
                double roll = Rng.NextDouble() * totalWeight; // Create a random generator

                double runningTotal = 0.0;
                List<Character> chosenGroup = aliveEnemies;

                // Loop through the weighted groups and find the target based on the random roll
                foreach (var group in weightedGroups)
                {
                    runningTotal += group.Weight;

                    if (roll < runningTotal)
                    {
                        chosenGroup = group.Targets;
                        break;
                    }
                }
                target = GetLowestHealthTarget(chosenGroup);
            }

            gnomeEater.turnCounter++; // Increment turn counter each time the Gnome Eater attacks
            return new CombatRequest // Create and return a CombatRequest with the chosen action and target
            {
                Action = chosenAction,
                TargetCharacterId = target.Id
            };
        }
    }
}
