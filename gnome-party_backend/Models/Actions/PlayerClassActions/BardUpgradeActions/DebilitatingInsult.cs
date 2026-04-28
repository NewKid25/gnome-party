using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.PlayerClassActions.BardUpgradeActions
{
    //  Debilitating Insult: Deal 6 damage to a target and makes the mocked enemy target you for reduced damage
    public sealed class DebilitatingInsult : CharacterAction
    {
        public DebilitatingInsult() : base("Debilitating Insult") // Pass the name of the action to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Debilitating Insult", "Deal 6 damage but causes the mocked enemy to target you for reduced damage");
        }

        // Override the ResolveAttack method to define the behavior of the Debilitating Insult Action
        public override AttackResolution ResolveAttack(
            Character user,
            Character target,
            CombatEncounterGameState gameState,
            bool isRedirected = false,
            bool isUnblockable = false)
        {
            double weakenedAmount = 0.5; // Define the damage reduction for the weakened status (In this case, 50% damage reduction)

            // Add validation to ensure that the user, target, and gameState are not null
            if (user == null) { throw new ArgumentNullException(nameof(user)); }
            if (target == null) { throw new ArgumentNullException(nameof(target)); }
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            var resolution = new AttackResolution(); // Creare a new attack resolution to hold the results of the attack

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Any(c => c.Id == target.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); }

            int debilitingInsultDamage = 6;
            // Create a new AttackInstance for the Mockery attack and add it to the resolution
            resolution.AttackInstances = new List<AttackInstance>
            {
                new AttackInstance
                {
                    ActionName = AttackName,
                    BaseDamage = debilitingInsultDamage,
                    FinalDamage = debilitingInsultDamage,
                    SourceCharacterId = user.Id,
                    TargetCharacterId = target.Id,
                }
            };

            // Apply the mocked status effect to the target
            resolution.StatusEffectsToApply.Add(new MockStatus(user, target));
            resolution.Events.Add(new CombatEvent("mock_status_applied", new StatusAppliedEventParams { OwnerId = user.Id }));

            // Apply the weakened status effect to the target
            resolution.StatusEffectsToApply.Add(new WeakenedStatus(user, target, weakenedAmount));
            resolution.Events.Add(new CombatEvent("weakened_status_applied", new StatusAppliedEventParams { OwnerId = user.Id }));
            return resolution;
        }
    }
}
