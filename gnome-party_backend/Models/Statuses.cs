namespace Models
{
    public abstract class StatusEffectBase : IStatusEffect
    {
        public abstract string StatusName { get; }
        public int RoundsRemaining { get; protected set; }
        public bool IsExpired
        {
            get { return RoundsRemaining <= 0; }
        }
        protected StatusEffectBase(int rounds)
        {
            RoundsRemaining = rounds;
        }
        public virtual void OnRoundStart(Character_Base owner) { }
        public virtual void OnRoundEnd(Character_Base owner) { }
        public virtual void OnBeforeBeingAttacked(Character_Base owner, AttackContext context) { }
        public virtual void OnModifyIncomingDamage(Character_Base owner, AttackContext context) { }   
        public virtual void ProgressRound() { RoundsRemaining--;}
    }
    public sealed class RedirectAttackToCaster_Status : StatusEffectBase
    {
        private readonly Character_Base _protector;
        public RedirectAttackToCaster_Status(Character_Base protector, int rounds) : base(rounds) { _protector = protector;}
        public override void OnBeforeBeingAttacked(Character_Base owner, AttackContext context)
        {
            if(_protector == null)
            {
                return;
            }
            if(_protector.Health <= 0)
            {
                return;

            }
            context.CurrentTarget = _protector;
        }
        public override string StatusName
        {
            get { return "Protected"; }
        }
    }
    public sealed class DamageReduction_Status : StatusEffectBase
    {
        private readonly double _multiplier;
        public DamageReduction_Status(double multiplier, int rounds) : base(rounds) { _multiplier = multiplier; }
        public override string StatusName
        {
            get { return "Reduced Damage"; }
        }
        public override void OnModifyIncomingDamage(Character_Base owner, AttackContext context)
        {
            context.ModifiedDamage = (int)Math.Floor(context.ModifiedDamage * _multiplier);
        }
    }
}
