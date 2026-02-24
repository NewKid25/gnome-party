using System;
using System.Diagnostics.Contracts;

namespace CombatService
{
    public sealed class AttackContext
    {
        public Character_Base Attacker { get; private set; }
        public AttackAction Attack { get; private set; }
        public Character_Base OriginalTarget { get; private set; }
        public Character_Base CurrentTarget { get; set; }
        public int BaseDamage { get; set; }
        public int ModifiedDamage { get; set; }
        public int HitIndex { get; private set; }
        public AttackContext(Character_Base contextAttacker, AttackAction contextAttack, Character_Base contextTarget, int contextHitIndex)
        {
            if(contextAttacker == null)
            {
                throw new ArgumentNullException("attacker");
            }
            if(contextAttack == null)
            {
                throw new ArgumentNullException("attack");
            }
            if(contextTarget == null)
            {
                throw new ArgumentNullException("target");
            }
            Attacker = contextAttacker;
            Attack = contextAttack;
            OriginalTarget = contextTarget;
            CurrentTarget = contextTarget;
            HitIndex = contextHitIndex;
        }
        public AttackContext(Character_Base attacker, AttackAction attack, Character_Base target) : this(attacker, attack, target, 0) {}
    }
}
