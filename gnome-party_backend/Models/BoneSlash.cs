using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actions
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
