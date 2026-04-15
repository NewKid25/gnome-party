using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Actions.EasyEnemyPoolActions.SkeletonActions
{
    // Bone Slash: A basic melee attack that deals 6 damage to a single target.
    public sealed class BoneSlash : CharacterAction
    {
        public BoneSlash() : base("Bone Slash") // Call the base constructor with the action name
        {
            ActionDescription = new CharacterActionDescription("Bone Slash", "Deal 6 damage to target enemy"); // Set the action description
        }
        public override AttackResolution ResolveAttack(Character user, // Override the ResolveAttack method to define the attack's behavior
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected, 
            bool isUnblockable)
        {
            if (user == null) throw new ArgumentNullException(nameof(user)); // Validate that the user character is not null
            if (target == null) throw new ArgumentNullException(nameof(target)); // Validate that the target character is not null
            if(gameState == null) throw new ArgumentNullException(nameof(gameState)); // Validate that the gameState is not null

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Any(c => c.Id == target.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); }

            return new AttackResolution // Return an AttackResolution object that describes the outcome of the attack
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
