using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Actions.BossPoolActions.NecrognomancerActions
{
    // Soul Drain: Deal 6 damage to all enemies and then heal for the amount of damage done
    public sealed class SoulDrain : CharacterAction
    {
        public SoulDrain() : base("Soul Drain") // Call the base constructor with the name of the action
        {
            ActionDescription = new CharacterActionDescription("Soul Drain", "Deal 6 damage to all enemies and heal for total damage done"); // Set the action description
        }

        // Override the ResolveAttack method to implement the logic for dealing damage to all enemies
        public override AttackResolution ResolveAttack(
            Character user,
            Character target,
            CombatEncounterGameState gameState,
            bool isRedirected = false,
            bool isUnblockable = false)
        {
            // Add validation to ensure that the user, target, and gameState are not null
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            // Initialize a list to hold the targets of the Whirling Strike
            List<Character> soulDrainTargets = TargetingService.GetOpposingTeam(gameState, user.Id);
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState); // Validate that the targets are eligible for this attack

            var resolution = new AttackResolution(); // Create a new AttackResolution object to hold the results of the attack
            int soulDrainDamage = 6;

            // Iterate through each target and create an AttackInstance for each one
            foreach (var currentTarget in soulDrainTargets)
            {
                // validate that the target is eligible for this attack
                if (!eligibleTargets.Any(c => c.Id == currentTarget.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(currentTarget)); }

                resolution.AttackInstances.Add(new AttackInstance
                {
                    ActionName = AttackName,
                    BaseDamage = soulDrainDamage,
                    FinalDamage = soulDrainDamage,
                    IsRedirected = isRedirected,
                    SourceCharacterId = user.Id,
                    TargetCharacterId = currentTarget.Id,
                });
            }

            // Real healing will be calculated in the CombatService
            resolution.HealInstances.Add(new HealInstance
            {
                SourceCharacterId = user.Id,
                TargetCharacterId = user.Id,
                ActionName = AttackName,
                CalculationType = HealCalculationType.FromDamageDealt,
                HealingRatio = 1.0,
                DamageSourceActionNameFilter = AttackName,
                RequireSameSourceCharacter = true
            });

            return resolution;
        }
    }
}
