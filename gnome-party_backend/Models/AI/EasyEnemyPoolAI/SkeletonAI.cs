using System.Diagnostics;
using Microsoft.VisualBasic;
using Models.CharacterData;
using Models.CombatData;
using Models.TestHelperData;

namespace Models.AI.EasyEnemyPoolAI;

internal class SkeletonAI : CharacterAI
{
    public SkeletonAI() { }
    public SkeletonAI(IRandomGenerator rng) : base(rng) { }
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

        // Verify if all Skeleton Actions are present
        bool hasBoneSlash = actions.Contains("Bone Slash"); 
        bool hasRattleGuard = actions.Contains("Rattle Guard");

        string chosenAction = null; // Variable to hold the chosen action

        double healthPercentage = (double)self.Health / Math.Max(1, self.MaxHealth); // Calculate the health percentage of the Skeleton, using Math.Max to avoid division by zero

        // If health is at or below 30%, "Rattle Guard" is available. And if a random chance check passes (40% chance), choose "Rattle Guard"
        if (healthPercentage <= 0.3 && hasRattleGuard && Rng.NextDouble() <= 0.4) {  chosenAction = "Rattle Guard"; }
        else if (hasBoneSlash) { chosenAction = "Bone Slash"; }
        else  { chosenAction = GetDefaultAction(actions); }

        var target = GetLowestHealthTarget(aliveEnemies); // Call a help method to target the enemy with the lowest health

        return new CombatRequest // Create and return a CombatRequest with the chosen action and target
        {
            Action = chosenAction,
            TargetCharacterId = target.Id
        };
    }
}
