using Models.CharacterData;
using Models.CombatData;

namespace Models.AI;

internal abstract class CharacterAI
{
    public abstract CombatRequest ChooseAction(List<string> actions, List<Character> enemies, List<Character> allies);

    protected string GetDefaultAction(List<string> actions)
    {
        return actions[0];
    }
}
