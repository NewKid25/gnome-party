using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.TestHelperData;

namespace Models.AI.EasyEnemyPoolAI
{
    // Call 5 instance of Rng.
    //      1. 70% Chance to use Piercing Arrow on any player has 50% health or more
    //      2. 60% Chance to use Crippling Shot on any player is below 50% health
    //      3. Move decision tie breaker (50/50)
    //      4. Priority targeting roll for each player class:
    //          * 0 - 49%: Mage
    //          * 50 - 79%: Warrior
    //          * 80 - 100% Bard 
    //      5. Targteing Tie breaker roll
    internal class GoblinArcherAI : CharacterAI
    {
        public GoblinArcherAI() { }
        public GoblinArcherAI(IRandomGenerator rng) : base(rng) { }
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

            // Verify if all the Goblin Archer's actions are present in the provided actions list
            bool hasPiercingArrow = actions.Contains("Piercing Arrow");
            bool hasCripplingShot = actions.Contains("Crippling Shot");

            string chosenAction = null; // Variable to hold the chosen action

            // Create the random variable for decision making
            double piercingArrowChance = 0.7; // 70% chance to use Piercing Arrow (when at least one player's health is above 50)
            double piercingArrowRoll = Rng.NextDouble(); // Roll for Piercing Arrow
            double cripplingShotChance = 0.6; // 60% chance to use Crippling Shot (when at least one player's health is below 50)
            double cripplingShotRoll = Rng.NextDouble(); // Roll for Crippling Shot
            double randomActionRoll = Rng.NextDouble(); // Roll to randomly choose an action
            double targetingTieBreakerRoll = Rng.NextDouble(); // Roll for tie breaking between player characters

            // Find alive enemies with >= 50% health to target with Piercing Arrow
            double healthyTarget = 0.5;
            List<Character> piercingArrowTargets = aliveEnemies.Where(e => (double)e.Health / Math.Max(1, e.MaxHealth) >= healthyTarget).ToList();

            // Find alive enemies with < 50% health to target with Cruel Arrow
            double weakenedTarget = 0.5;
            List<Character> cruelArrowTarget = aliveEnemies.Where(e => (double)e.Health / Math.Max(1, e.MaxHealth) < weakenedTarget).ToList();

            // Run the specified priority rolls for Piercing Arrow and Crippling Shot before randomly choosing between the two
            if (hasPiercingArrow && piercingArrowTargets.Count > 0 && piercingArrowRoll <= piercingArrowChance) { chosenAction = "Piercing Arrow"; }
            else if (hasCripplingShot && cruelArrowTarget.Count > 0 && cripplingShotRoll <= cripplingShotChance) { chosenAction = "Crippling Shot"; }
            else if(hasPiercingArrow && hasCripplingShot)
            {
                if (randomActionRoll < 0.5) { chosenAction = "Piercing Arrow"; }
                else { chosenAction = "Crippling Shot"; }
            }
            else {chosenAction = GetDefaultAction(actions); }

            Character target = null;

            // Variables to decide which player classes have a higher priority chance to be targeted
            double mageTargetChance = 0.5;
            double warriorTargetChance = 0.3;
            double bardTargetChance = 0.2;

            // Separate alive enemies by character class
            List<Character> aliveMageTargets = aliveEnemies.Where(e => e is Mage).ToList();
            List<Character> aliveWarriorTargets = aliveEnemies.Where(e => e is Warrior).ToList();
            List<Character> aliveBardTargets = aliveEnemies.Where(e => e is Bard).ToList();

            // Create weighted/priority target groups for alive instance of player classes
            var weightedGroups = new List<(List<Character> Targets, double Weight)>();
            if (aliveMageTargets.Count > 0) { weightedGroups.Add((aliveMageTargets, mageTargetChance)); }
            if (aliveWarriorTargets.Count > 0) { weightedGroups.Add((aliveWarriorTargets, warriorTargetChance)); }
            if (aliveBardTargets.Count > 0) { weightedGroups.Add((aliveBardTargets, bardTargetChance)); }
            if (weightedGroups.Count == 0) { target = GetLowestHealthTarget(aliveEnemies); }
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

            Console.WriteLine($"Chosen Action: {chosenAction}");

            return new CombatRequest
            {
                Action = chosenAction,
                TargetCharacterId = target.Id
            };
        }

    }
}
