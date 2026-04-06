using Models.ActionMetaData;
using Models.CharacterData;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;

namespace Models.Actions.BardActions
{
    public sealed class InspiringSong : CharacterAction
    {
        public InspiringSong() : base("Inspiring Song") // Pass the name of the action to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Inspiring Song", "Buff an ally's damage"); // Set the action description
        }
        public override AttackResolution ResolveAttack(Character user, Character target, CombatEncounterGameState gameState, bool isRedirected = false, bool isUnblockable = false)
        {
            throw new NotImplementedException();
        }

        public override List<Character> ReturnEligibleTargets(Character user, Character target, CombatEncounterGameState gameState)
        {
            throw new NotImplementedException();
        }
    }
}