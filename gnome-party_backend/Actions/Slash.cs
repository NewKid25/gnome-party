using CombatService;
using Models;


namespace Actions
{
    public class Slash : CharacterAction
    {
        public Slash() : base("Slash") { }
        public override void ApplyEffect(Character_Base user, Character_Base target, AttackContext context)
        {
            int damage = 10;
            target.ReceiveDamage(damage, context);
        }
    }
}
