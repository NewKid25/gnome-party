using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.PlayerClassActions.WarriorActions
{
    public sealed class Parry : CharacterAction
    {
        // Parry: Target an enemy. Take no damage from that enemy this turn.
        public Parry() : base("Parry") // Pass the name to the base constructor
        {
            ActionDescription = new CharacterActionDescription( // Create the description
                "Parry",
                "Target an enemy. Take no damage from that enemy this turn."
            );
        }
        public override AttackResolution ResolveAttack( // Override the ResolveAttack method
            Character user,
            Character target,
            CombatEncounterGameState gameState,
            bool isRedirected = false,
            bool isUnblockable = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user)); // Validate that user is not null
            if (target == null) throw new ArgumentNullException(nameof(target)); // Validate that target is not null

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Any(c => c.Id == target.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); }

            var resolution = new AttackResolution(); // Create a new AttackResolution object
            resolution.StatusEffectsToApply.Add(new ParryStatus(user, target)); // Add a new ParryStatus to the StatusEffectsToApply list
            resolution.Events.Add(new CombatEvent("parry_status_applied", new StatusAppliedEventParams // Add a new CombatEvent to the Events list
            {
                OwnerId = user.Id
            }));
            return resolution;
        }

        // Override the ReturnEligibleTargets method to return all members of the opposing team as eligible targets
        public override List<Character> ReturnEligibleTargets(Character user, CombatEncounterGameState gameState)
        {
            List<Character> eligibleTargets = new List<Character>();
            eligibleTargets = TargetingService.GetOpposingTeam(gameState, user.Id);
            return eligibleTargets;
        }
    }
}