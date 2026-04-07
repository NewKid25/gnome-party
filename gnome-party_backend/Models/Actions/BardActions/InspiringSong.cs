using Models.ActionMetaData;
using Models.CharacterData;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.BardActions
{
    public sealed class InspiringSong : CharacterAction
    {
        public InspiringSong() : base("Inspiring Song") // Pass the name of the action to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Inspiring Song", "Buff an ally's damage"); // Set the action description
        }

        public override AttackResolution ResolveAttack(
            Character user, 
            Character ally, 
            CombatEncounterGameState gameState, 
            bool isRedirected = false, 
            bool isUnblockable = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user)); // Validate that the user character is not null
            if (ally == null) throw new ArgumentNullException(nameof(ally)); // Validate that the ally is not null
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Contains(ally)) { throw new ArgumentException("Target is not eligible for this attack", nameof(ally)); }

            // Apply the status effect to the ally
            var resolution = new AttackResolution();
            resolution.StatusEffectsToApply.Add(new InspiringSongStatus(user, ally));
            resolution.Events.Add(new CombatEvent("inspiring_song_applied", new StatusAppliedEventParams
            {
                OwnerId = ally.Id
            }));

            return resolution;
        }

        public override List<Character> ReturnEligibleTargets(Character user, CombatEncounterGameState gameState)
        {
            // Null check the user and gameState
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            return TargetingService.GetTargetsTeam(gameState, user.Id); // return the target's team
        }
    }
}