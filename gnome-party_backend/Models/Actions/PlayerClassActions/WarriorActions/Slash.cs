using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Actions.PlayerClassActions.WarriorActions
{
    // Slash: Deal 10 damage to target enemy
    public sealed class Slash : CharacterAction
    {
        public Slash() : base("Slash") // Pass the name of the action to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Slash", "Deal 10 damage to target enemy"); // Set the action description
        }
        public override AttackResolution ResolveAttack( // Override the ResolveAttack method to define the behavior of the Slash action
            Character user, 
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected = false, 
            bool isUnblockable = false)
        {
            // Add validation to ensure that the user, target, and gameState are not null
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Any(c => c.Id == target.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); }

            return new AttackResolution // Return an AttackResolution object that describes the result of the Slash action
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
                    }
                }
            };
        }
    }
}
