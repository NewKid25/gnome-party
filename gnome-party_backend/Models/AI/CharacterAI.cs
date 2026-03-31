using Models.CharacterData;
using Models.CombatData;

namespace Models.AI;

internal abstract class CharacterAI
{
    public abstract CombatRequest ChooseAction(Character self, List<string> actions, List<Character> enemies, List<Character> allies); // AI logic for who to target
    protected abstract string ChooseActionName(Character self, List<string> actions, List<Character> enemies, List<Character> allies); // AI logic for which action to choose
    protected string GetDefaultAction(List<string> actions)
    {
        return actions[0];
    }
}
