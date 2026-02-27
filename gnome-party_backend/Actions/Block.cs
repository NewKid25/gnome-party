using Models;
using Statuses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actions
{
    public sealed class Block : CharacterAction
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
