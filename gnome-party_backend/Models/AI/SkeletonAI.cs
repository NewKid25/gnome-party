using Models.CharacterData;
using Models.CombatData;

namespace Models.AI;

internal class SkeletonAI : CharacterAI
{
    public override CombatRequest ChooseAction(List<string> actions, List<Character> enemies, List<Character> allies)
    {
        var action = GetDefaultAction(actions);
        if (actions.Contains("Bone Slash"))
        {
            action = "Bone Slash";
        }
        var target = enemies[0]; 
        return new CombatRequest
        {
            Action = action,
            TargetCharacterId = target.Id
        };
    }
}
