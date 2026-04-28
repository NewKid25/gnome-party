using Models.ActionMetaData;
using Models.CharacterData;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.PlayerClassActions.BardUpgradeActions
{
    // Menacing Note: Deal 8 damage to a target, 
    public sealed class MenacingNote : CharacterAction
    {
        public MenacingNote() : base("Menacing Note") // Pass the name of the action to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Menacing Note", "Deal 8 damage to a target and psych out the enemy.");
        }
        // Override the ResolveAttack method to define the behavior of the Menancing Note Action
        public override AttackResolution ResolveAttack(
            Character user,
            Character target,
            CombatEncounterGameState gameState,
            bool isRedirected = false,
            bool isUnblockable = false)
        {
            double psychOutDamageMultiplier = 1.25; // Define a damage multiplier for the Psych Out status effect

            // Add validation to ensure that the user, target, and gameState are not null
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Any(c => c.Id == target.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); }

            var resolution = new AttackResolution(); // Create a new attack resolution to hold the results of the attack

            int menacingNoteDamage = 8;
            // Create a new AttackInstance for the Discord attack and add it to the resolution
            resolution.AttackInstances = new List<AttackInstance>
            {
                new AttackInstance
                {
                    ActionName = AttackName,
                    BaseDamage = menacingNoteDamage,
                    FinalDamage = menacingNoteDamage,
                    SourceCharacterId = user.Id,
                    TargetCharacterId = target.Id,
                }
            };

            // Apply the mocked status effect to the target
            resolution.StatusEffectsToApply.Add(new PsychOut(user, target, psychOutDamageMultiplier));
            resolution.Events.Add(new CombatEvent("psych_out_status_applied", new StatusAppliedEventParams { OwnerId = user.Id }));

            // Reset the song attach to Bard
            if (user is Bard resetBard)
            {
                resetBard.CurrentSong = "Soothing Song";
            }

            return resolution;
        }
    }
}
