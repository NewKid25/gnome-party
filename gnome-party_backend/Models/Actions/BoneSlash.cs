using Models;
using Models.CharacterData;

namespace Models.Actions
{
    public sealed class BoneSlash : CharacterAction
    {
        public BoneSlash() : base("Bone Slash") { }
        public override void ApplyEffect(Character user, Character target, AttackContext context)
        {
            int damage = 6;
            //temp solution
            target.Health -= damage;
            context.ModifiedDamage = damage;
        }
    }
}
