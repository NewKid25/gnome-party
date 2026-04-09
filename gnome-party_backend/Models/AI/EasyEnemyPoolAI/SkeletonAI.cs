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
        if (self == null)
        {
            throw new ArgumentNullException(nameof(self)); // Defensive check to ensure we have a reference to ourselves
        }
        if (actions == null || actions.Count == 0) // Defensive check to ensure we have actions to choose from
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

        // Verify if all Skeleton Actions are present
        bool hasBoneSlash = actions.Contains("Bone Slash"); 
        bool hasRattleGuard = actions.Contains("Rattle Guard");

        string chosenAction = null;

        double healthPercentage = (double)self.Health / Math.Max(1, self.MaxHealth); // Calculate the health percentage of the Skeleton, using Math.Max to avoid division by zero

        // If health is at or below 30%, "Rattle Guard" is available. And if a random chance check passes (40% chance), choose "Rattle Guard"
        if (healthPercentage <= 0.3 && hasRattleGuard && Rng.NextDouble() <= 0.4)
        {
            chosenAction = "Rattle Guard";
        }
        else if (hasBoneSlash)
        {
            chosenAction = "Bone Slash";
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

        return new CombatRequest // Create and return a CombatRequest with the chosen action and target
        {
            Action = chosenAction,
            TargetCharacterId = target.Id
        };
    }
}
