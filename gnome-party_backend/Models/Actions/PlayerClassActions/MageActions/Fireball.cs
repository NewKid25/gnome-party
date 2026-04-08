using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.PlayerClassActions.MageActions
{
    // Fireball: Deal 6 damage to the target and then burn the target and adjacent allies for 3 turns
    public sealed class Fireball : CharacterAction
    {
        public Fireball() : base("Fireball") // Call the base constructor with the name of the action
        {
            ActionDescription = new CharacterActionDescription("Fireball", "Deal damage to the target and then burn the target and adjacent allies for 3 turns"); // Set the action description
        }
        public override AttackResolution ResolveAttack( // Override the ResolveAttack method to define the behavior of the Fireball action
            Character user, 
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected = false, 
            bool isUnblockable = false)
        {
            // Set the burn duration and tick damage as constants
            const int burnDuration = 3; 
            const int burnTickDamage = 2;

            // Validate the user, target, and gameState parameters
            if (user == null) throw new ArgumentNullException(nameof(user)); 
            if (target == null) throw new ArgumentNullException(nameof(target));
            if(gameState == null) throw new ArgumentNullException(nameof(gameState));


            var resolution = new AttackResolution(); // Create a new AttackResolution object to store the results of the attack
            resolution.AttackInstances.Add(new AttackInstance // Add an AttackInstance to the resolution for the initial damage of the Fireball
            {
                ActionName = AttackName,
                BaseDamage = 6,
                FinalDamage = 6,
                IsRedirected = isRedirected,
                SourceCharacterId = user.Id,
                TargetCharacterId = target.Id,
            });
            List<Character> burnTargeets; // Determine the burn targets based on whether the attack was redirected or not
            if (isRedirected) // If the attack was redirected, only burn the original target, not adjacent allies
            {
                burnTargeets = new List<Character> { target };
            }
            else
            {
                burnTargeets = TargetingService.GetTargetAndAdjacentAllies(gameState, target.Id);
            }

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Any(c => c.Id == target.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); }

            foreach (var burnTarget in burnTargeets) // Apply the burn status effect to each burn target and add a combat event for the status application
            {
                resolution.StatusEffectsToApply.Add(new BurnStatus(user, burnTarget, burnDuration, burnTickDamage));
                resolution.Events.Add(new CombatEvent("burn_status_applied", new StatusAppliedEventParams
                {
                    OwnerId = burnTarget.Id,
                }));
            }
            return resolution;
        }
    }
}
