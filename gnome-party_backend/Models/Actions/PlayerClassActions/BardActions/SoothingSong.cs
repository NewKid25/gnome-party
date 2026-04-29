using Models.ActionMetaData;
using Models.CharacterData;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;

namespace Models.Actions.PlayerClassActions.BardActions
{
    // Soothing Song. Subset of Song. Heal an ally for 8 health.
    public sealed class SoothingSong : CharacterAction
    {
        public SoothingSong() : base("Soothing Song") // Pass the name of the action to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Soothing Song", "Heal an ally", CharacterActionTargetRule.Ally); // Set the action description
        }
        public override AttackResolution ResolveAttack(Character user, Character target, CombatEncounterGameState gameState, bool isRedirected = false, bool isUnblockable = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user)); // Validate the user, target, and gameState parameters
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Any(c => c.Id == target.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); }

            int soothingSongHealing = 8;
            var resolution = new AttackResolution(); // Create a new AttackResolution object to store the results of the attack
            resolution.HealInstances.Add(new HealInstance
            {
                ActionName = AttackName,
                BaseHealing = soothingSongHealing,
                FinalHealing = soothingSongHealing,
                SourceCharacterId = user.Id,
                TargetCharacterId = target.Id,
            });
            //move to next bard song
            ReplaceActionInUser(user, new InspiringSong());
            return resolution;
        }

        // Method to determine which targets are eligible for this attack
        public override List<Character> ReturnEligibleTargets(Character user, CombatEncounterGameState gameState)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));
            return TargetingService.GetTargetsTeam(gameState, user.Id);
        }

        private void ReplaceActionInUser(Character user, CharacterAction newAction)
        {
            // place new action in same slot as this action, by swapping out at same index
            var actionIndex = user.ActionsDescriptions.IndexOf(this.ActionDescription);
            Console.WriteLine($"Replacing action at index {actionIndex} with new action {newAction.AttackName}");
            user.ActionsDescriptions.RemoveAt(actionIndex);
            user.ActionsDescriptions.Insert(actionIndex, newAction.ActionDescription);

            // Emit event with updated character
            SocketEvents.RaiseActionUpdated(user);
        }
    }
}