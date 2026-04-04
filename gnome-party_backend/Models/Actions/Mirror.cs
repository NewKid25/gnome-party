using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions
{
    // Mirror: Target an enemy. Your next attack will also hit that enemy.
    public sealed class Mirror : CharacterAction
    {
        public Mirror() : base("Mirror") // Pass the name "Mirror" to the base constructor
        {
            ActionDescription = new CharacterActionDescription( "Mirror", "Target an enemy. Your next attack will also hit that enemy." ); // Set the action description
        }

        public override AttackResolution ResolveAttack( // Override the ResolveAttack method to define the behavior of the Mirror action
            Character user, 
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected = false, 
            bool isUnblockable = false)
        {
            if(user == null) throw new ArgumentNullException(nameof(user)); // Check if the user is null and throw an exception if it is
            if (target == null) throw new ArgumentNullException(nameof(target)); // Check if the target is null and throw an exception if it is

            var resolution = new AttackResolution(); // Create a new AttackResolution object to store the results of the action
            resolution.StatusEffectsToApply.Add(new MirrorStatus(user, target)); // Add a new MirrorStatus effect to the list of status effects to apply
            resolution.Events.Add(new CombatEvent("mirror_status_applied", new StatusAppliedEventParams /// Add a new CombatEvent to the list of events to trigger
            { 
                OwnerId = user.Id,

            }));
            return resolution;
        }
    }
}

