using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.WarriorActions
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
            if (user == null) throw new ArgumentNullException(nameof(user)); // Validate that the user is not null
            if (ally == null) throw new ArgumentNullException(nameof(ally)); // Validate that the ally is not null
            var resolution = new AttackResolution(); // Create a new AttackResolution object to store the results of the action
            resolution.StatusEffectsToApply.Add(new BlockStatus(user, ally)); // Add a new BlockStatus to the list of status effects to apply
            resolution.Events.Add(new CombatEvent("block_status_applied", new {sourceId = user.Id, ownerId = user.Id, targetId = ally.Id})); // Add a new combat event to indicate that the block status has been applied
            return resolution;
        }
    }
}
