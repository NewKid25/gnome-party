namespace Models
{
    public sealed class BoneSlash : CharacterAction
    {
        public BoneSlash() : base("Bone Slash") { }
        public override void ApplyEffect(Character_Base user, Character_Base target, AttackContext context)
        {
            int damage = 6;
            target.ReceiveDamage(damage, context);
        }
    }
}
