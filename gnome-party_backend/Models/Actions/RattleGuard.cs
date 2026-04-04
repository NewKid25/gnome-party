using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions
{
    public sealed class RattleGuard : CharacterAction
    {
        // Rattle Guard: Reduce damage by 50% for one turn
        public RattleGuard() : base("Rattle Guard") // Pass the name of the action to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Rattle Guard", "Reduce damage by 50% for one turn"); // Set the action description
        }
        public override AttackResolution ResolveAttack( // Override the ResolveAttack method to define the behavior of the Rattle Guard action
            Character user, 
            Character ally, 
            CombatEncounterGameState gameState, 
            bool isRedirected = false, 
            bool isUnblockable = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user)); // Validate that the user character is not null
            var resolution = new AttackResolution(); // Create a new AttackResolution object to hold the results of the action
            resolution.StatusEffectsToApply.Add(new RattleGuardStatus(user)); // Add the Rattle Guard status effect to the list of status effects to apply
            resolution.Events.Add(new CombatEvent("rattleguard_status_applied", new {sourceId = user.Id, ownerId = user.Id, targetId = user.Id })); // Add a combat event to indicate that the Rattle Guard status has been applied
            return resolution;
        }
    }
}
