using Models.ActionMetaData;
using Models.CharacterData;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.PlayerClassActions.BardActions
{
    public sealed class FrighteningSong : CharacterAction
    {
        public FrighteningSong() : base("Frightening Song") // Pass the name of the action to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Frightening Song", "Stun an enemy for a turn"); // Set the action description
        }

        public override AttackResolution ResolveAttack(Character user, Character target, CombatEncounterGameState gameState, bool isRedirected = false, bool isUnblockable = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user)); // Validate that the user character is not null
            if (target == null) throw new ArgumentNullException(nameof(target)); // Validate that the target character is not null
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Any(c => c.Id == target.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); }

            // Apply the stun status effect to the target
            var resolution = new AttackResolution();
            resolution.StatusEffectsToApply.Add(new StunStatus(target));
            resolution.Events.Add(new CombatEvent("stun_status_applied", new StatusAppliedEventParams
            {
                OwnerId = target.Id
            }));

            return resolution;
        }
    }
}
