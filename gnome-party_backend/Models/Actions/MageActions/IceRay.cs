using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.MageActions
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
            if (user == null) throw new ArgumentNullException(nameof(user)); // Validate that the user character is not null
            if (target == null) throw new ArgumentNullException(nameof(target)); // Validate that the target character is not null
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
