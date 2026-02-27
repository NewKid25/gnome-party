namespace Models
{
    public sealed class RattleGuard : CharacterAction
    {
        public RattleGuard() : base("Rattle Guard") { }
        public override void ApplyEffect(Character_Base user, Character_Base target, AttackContext context)
        {
            int rounds = 2; // for testing 2. In practice 1
            double multipier = 0.5;
            user.AddStatus(new DamageReduction_Status(multipier, rounds));
        }
    }
}
