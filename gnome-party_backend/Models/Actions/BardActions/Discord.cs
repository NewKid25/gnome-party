using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.CharacterData.PlayerCharacterClasses;

namespace Models.Actions.BardActions
{
    public sealed class Discord : CharacterAction
    {
        public Discord() : base("Discord") // Pass the name of the action to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Discord", "Deal 8 damage to a target and reset Song to Soothing Song");
        }

        // Override the ResolveAttack method to define the behavior of the Discord Action
        public override AttackResolution ResolveAttack(
            Character user, 
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected = false, 
            bool isUnblockable = false)
        {
            // Add validation to ensure that the user and target are not null
            if(user == null) throw new ArgumentNullException(nameof(user));
            if (target == null) throw new ArgumentNullException(nameof(target));

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Any(c => c.Id == target.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); }

            var resolution = new AttackResolution(); // Create a new attack resolution to hold the results of the attack

            // Create a new AttackInstance for the Discord attack and add it to the resolution
            resolution.AttackInstances = new List<AttackInstance>
            {
                new AttackInstance
                {
                    ActionName = AttackName,
                    BaseDamage = 8,
                    FinalDamage = 8,
                    SourceCharacterId = user.Id,
                    TargetCharacterId = target.Id,
                }
            };

            // Reset the song attach to Bard
            if(user is Bard resetBard)
            {
                resetBard.CurrentSong = "Soothing Song";
            }

            return resolution;
        }
    }
}
 