using Models.CharacterData;
using Models.CombatData;
using Models.TestHelperData;

namespace Models.AI;

internal abstract class CharacterAI
{
    public abstract CombatRequest ChooseAction(Character self, List<string> actions, List<Character> enemies, List<Character> allies); // AI logic for which action to use
    protected string GetDefaultAction(List<string> actions) { return actions[0]; } // Fallback action choice, can be overridden by specific AIs if needed
    protected IRandomGenerator Rng { get; } // Random number generator for any AI that needs to make random decisions
    protected CharacterAI() : this(new RandomNumGen()) { } // Default constructor uses the default random number generator
    protected CharacterAI(IRandomGenerator rng) {if (rng == null) throw new ArgumentNullException(nameof(rng)); Rng = rng; } // Constructor that allows for dependency injection of a random number generator, useful for testing
    protected Character GetLowestHealthTarget(List<Character> aliveEnemies) // Helper method to get the lowest health enemy to target
    {
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

        return target;
    }
}
