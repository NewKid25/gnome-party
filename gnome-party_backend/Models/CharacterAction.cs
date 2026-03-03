namespace GnomeParty.Models
{
    public abstract class CharacterAction
    {
        public string AttackName { get; private set; }
        public bool Unblockable { get; private set; }
        public CharacterActionDescription ActionDescription { get; set; }
        protected CharacterAction(string attackName, bool unblockable)
        {
            AttackName = attackName;
            Unblockable = unblockable;
            ActionDescription = new CharacterActionDescription(attackName);
        }
        protected CharacterAction(string attackName) : this(attackName, false) { }
        public abstract void ApplyEffect(Character_Base user, Character_Base target, AttackContext context);
    }
}
