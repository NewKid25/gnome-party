using Models.CharacterData;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.TestHelperData;

namespace Models.AI;

internal abstract class CharacterAI
{
    public abstract CombatRequest ChooseAction(Character self, List<string> actions, List<Character> enemies, List<Character> allies); // AI logic for which action to use

    // Special case for Enemy AI that need player request info (I.e Forest Sprite)
    public virtual CombatRequest ChooseAction(Character self, List<string> actions, List<Character> enemies, List<Character> allies, List<CombatRequest> playerRequests) 
    {
        return ChooseAction(self, actions, enemies, allies);
    }
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
    protected static readonly HashSet<string> HighImpactActions = new()
    {
        "Whirling Strike",
        "Song",
    }; // List of actions deemed "High Impact"
    protected static bool IsHighImpactAction(List<CombatRequest> playerRequests) // Method to see if an action is "High Impact"
    {
        if (playerRequests == null || playerRequests.Count == 0) { return false; } // Check that a valid player request was sent
        
        // Loop through each request to see if a high impact move was used
        foreach (var request in playerRequests)
        {
            if (request == null) { continue; }
            if (string.IsNullOrWhiteSpace(request.Action)){ continue; }
            if (HighImpactActions.Contains(request.Action)) { return true; }
        }
        return false;
    }
    protected Character GetHighImpactUser(List<CombatRequest> playerRequests, List<Character> aliveEnemies)
    {
        if (playerRequests == null || playerRequests.Count == 0) { return null; }
        var highImpactUsers = new List<Character>();
        foreach (var request in playerRequests)
        {
            if (request == null) { continue; } // null check each combat request
            if (string.IsNullOrWhiteSpace(request.Action)) { continue; } // null check action within the combat request
            if (!HighImpactActions.Contains(request.Action)) { continue; } // check if the non null action is a high impact action
            var validMatch = aliveEnemies.FirstOrDefault(e => e.Id == request.SourceCharacterId); // store an instance of a character who is using a high impact action
            if (validMatch != null) { highImpactUsers.Add(validMatch); } // add valid matches to a list variable
        }
        if (highImpactUsers.Count == 0) { return null; } // return null if no high impact action users were found
        
        var target = new Character();
        
        double bardTargetChance = 0.5;
        double mageTargetChance = 0.3;
        double warriorTargetChance = 0.2;

        // Separate alive enemies by character class
        List<Character> aliveBardTargets = aliveEnemies.Where(e => e is Bard).ToList();
        List<Character> aliveMageTargets = aliveEnemies.Where(e => e is Mage).ToList();
        List<Character> aliveWarriorTargets = aliveEnemies.Where(e => e is Warrior).ToList();

        // Create weighted/priority target groups for alive instance of player classes
        var weightedGroups = new List<(List<Character> Targets, double Weight)>();
        if (aliveBardTargets.Count > 0) { weightedGroups.Add((aliveBardTargets, bardTargetChance)); }
        if (aliveMageTargets.Count > 0) { weightedGroups.Add((aliveMageTargets, mageTargetChance)); }
        if (aliveWarriorTargets.Count > 0) { weightedGroups.Add((aliveWarriorTargets, warriorTargetChance)); }
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
        return target;
    }
}
