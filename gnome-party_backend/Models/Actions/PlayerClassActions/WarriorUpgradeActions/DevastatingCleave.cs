using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Actions.PlayerClassActions.WarriorActions
{
    // Devastating Cleave: Deal 8 damage to all enemies.
    public sealed class DevastatingCleave : CharacterAction
    {
        public DevastatingCleave() : base("Devastating Cleave") // Call the base constructor with the name of the action
        {
            ActionDescription = new CharacterActionDescription("Devastating Cleave", "Deal 8 damage to all enemies"); // Set the action description
        }

        // Override the ResolveAttack method to implement the logic for dealing damage to all enemies
        public override AttackResolution ResolveAttack(
            Character user,
            Character target,
            CombatEncounterGameState gameState,
            bool isRedirected = false,
            bool isUnblockable = false)
        {
            int deastatingCleaveDamage = 8;
            // Add validation to ensure that the user, target, and gameState are not null
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            // Initialize a list to hold the targets of the Whirling Strike
            List<Character> whirlStrikeTargets = TargetingService.GetOpposingTeam(gameState, user.Id);

            var resolution = new AttackResolution(); // Create a new AttackResolution object to hold the results of the attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState); // Validate that the targets are eligible for this attack

            // Iterate through each target and create an AttackInstance for each one
            foreach (var currentTarget in whirlStrikeTargets)
            {
                // validate that the target is eligible for this attack
                if (!eligibleTargets.Any(c => c.Id == currentTarget.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(currentTarget)); }

                resolution.AttackInstances.Add(new AttackInstance
                {
                    ActionName = AttackName,
                    BaseDamage = deastatingCleaveDamage,
                    FinalDamage = deastatingCleaveDamage,
                    IsRedirected = isRedirected,
                    SourceCharacterId = user.Id,
                    TargetCharacterId = currentTarget.Id,
                });
            }
            return resolution;
        }
    }
}
