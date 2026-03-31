using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Actions
{
    public class BoneSlash : CharacterAction
    {
        public BoneSlash() : base("Bone Slash")
        {
            ActionDescription = new CharacterActionDescription("Bone Slash", "Deal 6 damage to target enemy");
        }
        public override void ApplyEffect(Character user, Character target, AttackContext context)
        {
            throw new NotImplementedException();
        }

        public override AttackResolution ResolveAttack(Character user, Character target, CombatEncounterGameState gameState, bool isRedirected = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (target == null) throw new ArgumentNullException(nameof(target));
            return new AttackResolution
            {
                AttackInstances = new List<AttackInstance>
                {
                    new AttackInstance
                    {
                        SourceCharacterId = user.Id,
                        TargetCharacterId = target.Id,
                        ActionName = AttackName,
                        BaseDamage = 6,
                        FinalDamage = 6
                    }
                }
            };
        }
    }
}
