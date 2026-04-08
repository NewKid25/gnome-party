using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Actions.PlayerClassActions.MageActions
{
    // Magic Missile: Deal 10 damage to target enemy uninterrupted
    public sealed class MagicMisslie : CharacterAction
    {
        public MagicMisslie() : base("Magic Missile") // Pass the action name to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Magic Missile", "Deal 10 damage to target enemy uninterrupted"); // Set the action description
        }
        public override AttackResolution ResolveAttack( // Override the ResolveAttack method to implement the specific logic for Magic Missile
            Character user, 
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected, 
            bool unblockable)
        {
            // Add validation to ensure that the user, target, and gameState are not null
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Any(c => c.Id == target.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); }

            return new AttackResolution // Return a new AttackResolution object with the details of the attack
            {
                AttackInstances = new List<AttackInstance>
                {
                    new AttackInstance
                    {
                        ActionName = AttackName,
                        BaseDamage = 10,
                        FinalDamage = 10,
                        SourceCharacterId = user.Id,
                        TargetCharacterId = target.Id,
                        IsUnblockable = true,
                        IsBlocked = false,
                    }
                }
            };
        }
    }
}
