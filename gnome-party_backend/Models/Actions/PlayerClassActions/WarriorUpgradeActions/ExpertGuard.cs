using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.PlayerClassActions.WarriorUpgradeActions
{
    public sealed class ExpertGuard : CharacterAction
    {
        public ExpertGuard() : base("Expert Guard") // Pass the name of the action to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Expert Guard", "Guard an ally for even more reduced"); // Set the action description
        }
        public override AttackResolution ResolveAttack( // Override the ResolveAttack method to implement the action's effect
            Character user,
            Character ally,
            CombatEncounterGameState gameState,
            bool isRedirected = false,
            bool isUnblockable = false)
        {
            // Add validation to ensure that the user, target, and gameState are not null
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (ally == null) throw new ArgumentNullException(nameof(ally));
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Contains(ally)) { throw new ArgumentException("Target is not eligible for this attack", nameof(ally)); }

            var resolution = new AttackResolution(); // Create a new AttackResolution object to store the results of the action
            double reduction = 0.75;
            resolution.StatusEffectsToApply.Add(new BlockStatus(user, ally, reduction)); // Add a new BlockStatus to the list of status effects to apply
            resolution.Events.Add(new CombatEvent("block_status_applied", new { sourceId = user.Id, ownerId = user.Id, targetId = ally.Id })); // Add a new combat event to indicate that the block status has been applied
            return resolution;
        }

        public override List<Character> ReturnEligibleTargets(Character user, CombatEncounterGameState gameState)
        {
            if (user == null) { throw new ArgumentNullException(nameof(user)); }
            if (gameState == null) { throw new ArgumentNullException(nameof(gameState)); }
            return TargetingService.GetTargetsTeam(gameState, user.Id);
        }
    }
}
