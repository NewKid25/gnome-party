using Models.CharacterData;
using Models.CombatData;

namespace Models.ActionMetaData
{
    public interface IMultiTargetAction
    {
        AttackResolution ResolveAttack(
            Character user,
            List<Character> targets,
            CombatEncounterGameState gameState,
            bool isRedirected = false,
            bool isUnblockable = false);

        int MaxTargets { get; }
        int MinTargets { get; }
    }
}
