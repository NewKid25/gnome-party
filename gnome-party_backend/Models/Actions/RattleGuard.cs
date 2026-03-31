using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Actions
{
    public sealed class RattleGuard : CharacterAction
    {
        public RattleGuard() : base("Rattle Guard") { }
        public override void ApplyEffect(Character user, Character target, AttackContext context)
        {
            int rounds = 2; // for testing 2. In practice 1
            double multipier = 0.5;
            //user.AddStatus(new DamageReduction_Status(multipier, rounds));
        }

        public override AttackResolution ResolveAttack(Character user, Character target, CombatEncounterGameState gameState, bool isRedirected = false)
        {
            throw new NotImplementedException();
        }
    }
}
