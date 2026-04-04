using System.Diagnostics;
using Models.CharacterData;
using Models.CombatData;

namespace Models.AI;

internal class SkeletonAI : CharacterAI
{
    public override CombatRequest ChooseAction(Character self, List<string> actions, List<Character> enemies, List<Character> allies)
    {
        if(actions == null || actions.Count == 0) // Defensive check to ensure we have actions to choose from
        {
            throw new ArgumentException("Actions list cannot be null or empty.");
        }
        if(enemies == null || enemies.Count == 0) // Defensive check to ensure we have enemies to target
        {
            throw new ArgumentException("Enemies list cannot be null or empty.");
        }
        var aliveEnemies = enemies.Where(e => e.Health > 0).ToList(); // Filter out dead enemies
        if (aliveEnemies.Count == 0) // If there are no alive enemies, we can't target anyone, so we should handle this case appropriately
        {
            throw new InvalidOperationException("No alive enemies to target.");
        }
        var action = ChooseActionName(self, actions, enemies, allies); // Determine which action to take based on the current state

        // Target the enemy with the lowest health percentage, then lowest absolute health, then highest max health as a tiebreaker
        var target = aliveEnemies.OrderBy(e => (double)e.Health / Math.Max(1, e.MaxHealth)).ThenBy(e => e.Health).ThenByDescending(e => e.MaxHealth).First(); 
        return new CombatRequest // Create and return a CombatRequest with the chosen action and target
        {
            Action = action,
            TargetCharacterId = target.Id
        };
    }
    protected override string ChooseActionName(Character self, List<string> actions, List<Character> enemies, List<Character> allies)
    {
        bool hasBoneSlash = actions.Contains("Bone Slash"); // Check if "Bone Slash" is available in the actions list
        bool hasRattleGuard = actions.Contains("Rattle Guard"); // Check if "Rattle Guard" is available in the actions list

        double healthPercentage = (double)self.Health / Math.Max(1, self.MaxHealth); // Calculate the health percentage of the Skeleton, using Math.Max to avoid division by zero
        if (healthPercentage <= 0.3 && hasRattleGuard && Rng.NextDouble() <= 0.4) // If health is at or below 30%, "Rattle Guard" is available, and a random chance check passes (40% chance), choose "Rattle Guard"
        {
            return "Rattle Guard";
        }
        if(hasBoneSlash)
        {
            return "Bone Slash";
        }
        return GetDefaultAction(actions);
    }
}
