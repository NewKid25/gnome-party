using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatService
{
    public sealed class Slash : AttackAction
    {
        public Slash() : base("Slash") { }
        public override void ApplyEffect(Character_Base user, Character_Base target, AttackContext context)
        {
            int damage = 10;
            target.ReceiveDamage(damage, context);
        }
    }
    public sealed class Block : AttackAction
    {
        public Block() : base("Block") { }
        public override void ApplyEffect(Character_Base user, Character_Base ally, AttackContext context)
        {
            int rounds = 2; // for tsting 2. but in practice 1
            double multiplier = 0.5;
            ally.AddStatus(new RedirectAttackToCaster_Status(user, rounds));
            user.AddStatus(new DamageReduction_Status(multiplier, rounds));
        }
    }
}
