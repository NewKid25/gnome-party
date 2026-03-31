using Models.ActionMetaData;
using Models.CombatData;

namespace Models.CharacterData
{
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
        public abstract void ApplyEffect(Character user, Character target, AttackContext context);
        public abstract AttackResolution ResolveAttack(Character user, Character target, CombatEncounterGameState gameState, bool isRedirected = false, bool isUnblockable = false);
    }
}
