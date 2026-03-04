namespace GnomeParty.Models
{
    public class Slash : CharacterAction
    {
        public Slash() : base("Slash") { }
        public override void ApplyEffect(Character user, Character target, AttackContext context)
        {
            int damage = 10;
            //temp solution
            target.Health -= damage;
            //target.ReceiveDamage(damage, context);
        }
    }
}
