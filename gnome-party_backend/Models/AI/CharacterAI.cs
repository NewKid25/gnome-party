using Models.CharacterData;
using Models.CombatData;

namespace Models.AI;

internal abstract class CharacterAI
{
    public abstract CombatRequest ChooseAction(Character self, List<string> actions, List<Character> enemies, List<Character> allies); // AI logic for who to target
    protected abstract string ChooseActionName(Character self, List<string> actions, List<Character> enemies, List<Character> allies); // AI logic for which action to choose
    protected string GetDefaultAction(List<string> actions) { return actions[0]; } // Fallback action choice, can be overridden by specific AIs if needed
    internal interface IRandomProvider { double NextDouble(); } // Interface for providing random numbers, allows for easier testing and potential future extensions
    internal sealed class RandomGenerator : IRandomProvider { public double NextDouble() { return Random.Shared.NextDouble(); } } // Default implementation of IRandomProvider using System.Random
    protected IRandomProvider Rng { get; } // Constructor for CharacterAI, initializes the random generator. Allows for dependency injection of a custom random provider for testing or other purposes.
    protected CharacterAI() : this(new RandomGenerator()) { } // Default constructor that uses the default random generator
    protected CharacterAI(IRandomProvider randomProvider) { if (randomProvider == null) throw new ArgumentNullException(nameof(randomProvider)); Rng = randomProvider; } // Constructor that allows for injection of a custom random provider
}
