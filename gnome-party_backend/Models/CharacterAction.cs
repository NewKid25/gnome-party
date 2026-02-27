namespace Models
{
    public abstract class CharacterAction
    {
        public string AttackName { get; private set; }
        public bool Unblockable { get; private set; }
        protected CharacterAction(string attackName, bool unblockable)
        {
            AttackName = attackName;
            Unblockable = unblockable;
        }
        protected CharacterAction(string attackName) : this(attackName, false) { }
        public abstract void ApplyEffect(Character_Base user, Character_Base target, AttackContext context);
    }
}
