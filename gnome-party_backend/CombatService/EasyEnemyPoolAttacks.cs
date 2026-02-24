using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatService
{
    /********** SKELETON ATTACKS ************/
    public sealed class Bone_Slash : AttackAction
    {
        public Bone_Slash() : base("Bone Slash") { }
        public override void ApplyEffect(Character_Base user, Character_Base target, AttackContext context)
        {
            int damage = 6;
            target.ReceiveDamage(damage, context);
        }
    }
    public sealed class Rattle_Guard : AttackAction
    {
        public Rattle_Guard() : base("Rattle Guard") { }
        public override void ApplyEffect(Character_Base user, Character_Base target, AttackContext context)
        {
            int rounds = 2; // for testing 2. In practice 1
            double multipier = 0.5;
            user.AddStatus(new DamageReduction_Status(multipier, rounds));
        }
    }
    /******** GOBLIN ARCHER ATTACKS *********/
    /******** FOREST SPRITE ATTACKS *********/
}
