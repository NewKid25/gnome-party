using System;
using System.Diagnostics.Contracts;
using Models.CharacterData;

namespace Models.ActionMetaData
{
    public sealed class AttackContext
    {
        public CharacterAction Attack { get; private set; }
        public AttackContext(Character attacker, CharacterAction attack, Character target) : this(attacker, attack, target, 0) {}
        public AttackContext(Character contextAttacker, CharacterAction contextAttack, Character contextTarget, int contextHitIndex)
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
        public Character Attacker { get; private set; }
        public int BaseDamage { get; set; }
        public Character CurrentTarget { get; set; }
        public int HitIndex { get; private set; }
        public int ModifiedDamage { get; set; }
        public Character OriginalTarget { get; private set; }
    }
}
