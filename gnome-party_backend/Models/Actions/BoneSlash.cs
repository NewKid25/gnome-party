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
        public override AttackResolution ResolveAttack(Character user, 
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected = false, 
            bool isUnblockable = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (target == null) throw new ArgumentNullException(nameof(target));
            return new AttackResolution
            {
                AttackInstances = new List<AttackInstance>
                {
                    new AttackInstance
                    {
                        ActionName = AttackName,
                        BaseDamage = 6,
                        FinalDamage = 6,
                        SourceCharacterId = user.Id,
                        TargetCharacterId = target.Id,
                    }
                }
            };
        }
    }
}
