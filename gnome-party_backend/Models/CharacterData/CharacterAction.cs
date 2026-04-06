using Models.ActionMetaData;
using Models.CombatData;

namespace Models.CharacterData
{
    // Base class for character actions
    public abstract class CharacterAction 
    {
        public string AttackName { get; private set; }
        public bool Unblockable { get; private set; }
        protected CharacterAction(string attackName) : this(attackName, false) { }
        protected CharacterAction(string attackName, bool unblockable)
        {
            AttackName = attackName;
            Unblockable = unblockable;
            ActionDescription = new CharacterActionDescription(attackName);
        }
        public CharacterActionDescription ActionDescription { get; set; }
        public abstract AttackResolution ResolveAttack(Character user, Character target, CombatEncounterGameState gameState, bool isRedirected = false, bool isUnblockable = false);
        public virtual List<Character> ReturnEligibleTargets(Character user, CombatEncounterGameState gameState)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));
            return TargetingService.GetOpposingTeam(gameState, user.Id);
        }
    }
}
