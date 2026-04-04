using Models.CharacterData;
using Models.CombatData;
using Models.TestHelperData;

namespace Models.AI;

internal abstract class CharacterAI
{
    public abstract CombatRequest ChooseAction(Character self, List<string> actions, List<Character> enemies, List<Character> allies); // AI logic for who to target
    protected abstract string ChooseActionName(Character self, List<string> actions, List<Character> enemies, List<Character> allies); // AI logic for which action to choose
    protected string GetDefaultAction(List<string> actions) { return actions[0]; } // Fallback action choice, can be overridden by specific AIs if needed
    protected IRandomGenerator Rng { get; } // Random number generator for any AI that needs to make random decisions
    protected CharacterAI() : this(new RandomNumGen()) { } // Default constructor uses the default random number generator
    protected CharacterAI(IRandomGenerator rng) {if (rng == null) throw new ArgumentNullException(nameof(rng)); Rng = rng; } // Constructor that allows for dependency injection of a random number generator, useful for testing
}
