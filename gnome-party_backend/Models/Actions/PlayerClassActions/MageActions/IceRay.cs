using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.PlayerClassActions.MageActions
{
    // Ice Ray: Deals 5 damage and applies a chill status effect to the target, reducing their attack power 
    public sealed class IceRay : CharacterAction
    {
        public IceRay() : base("Ice Ray") // Pass the name of the action to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Ice Ray", "Deal 5 damage to a target and reduce their attack power."); // Set the action description
        }
        public override AttackResolution ResolveAttack( // Override the ResolveAttack method to define the behavior of the Ice Ray action
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

            var resolution = new AttackResolution(); // Create a new AttackResolution object to hold the results of the attack
            resolution.AttackInstances = new List<AttackInstance> // Create a new AttackInstance for the Ice Ray attack and add it to the resolution
            {
                new AttackInstance
                {
                    ActionName = AttackName,
                    BaseDamage = 5,
                    FinalDamage = 5,
                    SourceCharacterId = user.Id,
                    TargetCharacterId = target.Id,
                }
            };
            resolution.StatusEffectsToApply.Add(new ChillStatus(user, target)); // Add a new ChillStatus effect to the list of status effects to apply to the target
            resolution.Events.Add(new CombatEvent("chiil_status_applied", new StatusAppliedEventParams{ OwnerId = user.Id })); // Add a new combat event to indicate that the chill status has been applied
            return resolution;
        }
    }
}
