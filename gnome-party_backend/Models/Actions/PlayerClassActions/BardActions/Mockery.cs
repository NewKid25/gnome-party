using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.PlayerClassActions.BardActions
{
    // Mockery: Deal 6 damage to a target, but makes the mocked enemy target you
    public sealed class Mockery : CharacterAction
    {
        public Mockery() : base ("Mockery") // Pass the name of the action to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Mockery", "Deal 6 damage but causes the mocked enemy to target you");
        }

        // Override the ResolveAttack method to define the behavior of the Mockery Action
        public override AttackResolution ResolveAttack(
            Character user, 
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected = false,
            bool isUnblockable = false)
        {
            // Add validation to ensure that the user, target, and gameState are not null
            if(user == null) { throw new ArgumentNullException(nameof(user));}
            if(target == null) { throw new ArgumentNullException(nameof(target));}
            if (gameState == null) throw new ArgumentNullException(nameof(gameState)); 

            var resolution = new AttackResolution(); // Creare a new attack resolution to hold the results of the attack

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Any(c => c.Id == target.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); }

            // Create a new AttackInstance for the Mockery attack and add it to the resolution
            resolution.AttackInstances = new List<AttackInstance>
            {
                new AttackInstance
                {
                    ActionName = AttackName,
                    BaseDamage = 6,
                    FinalDamage = 6,
                    SourceCharacterId = user.Id,
                    TargetCharacterId = target.Id,
                }
            };

            // Apply the mocked status effect to the target
            resolution.StatusEffectsToApply.Add(new MockStatus(user, target));
            resolution.Events.Add(new CombatEvent("mock_status_applied", new StatusAppliedEventParams { OwnerId = user.Id }));

            return resolution;
        }
    }
}
