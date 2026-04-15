using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;
using Models.CharacterData.DifficultEnemyPoolClasses;
using Models.CombatData;
using Models.TestHelperData;

namespace Models.AI.DifficultEnemyPoolAI
{
    internal class GnombieBruteAI : CharacterAI
    {
        // Calls 2 instance of Rng
        // Rotting Aura Chance: 50%
        // Targeting Random

        public GnombieBruteAI() { }
        public GnombieBruteAI(IRandomGenerator rng) : base(rng) { }
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

            if(self is not GnombieBrute gnombieBrute) { throw new InvalidCastException("Character is not a Gnombie Brute"); }

            // Verify if all the Gnombie Brute actions are present
            bool hasHeavySlam = actions.Contains("Heavy Slam");
            bool hasRottingAura = actions.Contains("Rotting Aura");

            string chosenAction = null; // Variable to hold the chosen action

            // Variables for using Rotting Aura 
            double rottingAuraChance = 0.5;
            double rottingAuraRoll = Rng.NextDouble();
            int rottingAuraRequiredTurnCount = 3;

            // Choose Rotting Aura on a successful Rotting Aura roll and on round 3
            if(hasRottingAura 
                && rottingAuraRoll <= rottingAuraChance 
                && gnombieBrute.turnCount > 0 
                && (gnombieBrute.turnCount % rottingAuraRequiredTurnCount == 0))
            {
                chosenAction = "Rotting Aura";
            }
            else if(hasHeavySlam) { chosenAction = "Heavy Slam"; }
            else { chosenAction = GetDefaultAction(actions); }

            var target = GetRandomTarget(aliveEnemies);

            gnombieBrute.turnCount++;

            return new CombatRequest // Create and return a CombatRequest with the chosen action and target
            {
                Action = chosenAction,
                TargetCharacterId = target.Id
            };
        }
    }
}
