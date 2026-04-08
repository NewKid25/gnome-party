using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.PlayerClassActions.WarriorActions
{
    // Block: Target an ally and apply a Block status to them
    // BlockStatus: Redirect the damage from the next attack that would hit the ally to the user instead and reduce damage by 50%

    public sealed class Block : CharacterAction 
    {
        public Block() : base("Block") // Pass the name of the action to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Block", "Guard an ally"); // Set the action description
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
            resolution.StatusEffectsToApply.Add(new BlockStatus(user, ally)); // Add a new BlockStatus to the list of status effects to apply
            resolution.Events.Add(new CombatEvent("block_status_applied", new {sourceId = user.Id, ownerId = user.Id, targetId = ally.Id})); // Add a new combat event to indicate that the block status has been applied
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
