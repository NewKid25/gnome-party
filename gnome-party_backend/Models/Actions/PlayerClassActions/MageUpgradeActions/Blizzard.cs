using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions.PlayerClassActions.MageActions
{
    // Blizzard: Deals 5 damage and applies a chill status effect to 2 targeets reducing their attack power 
    public sealed class Blizzard : CharacterAction, IMultiTargetAction
    {
        public Blizzard() : base("Blizzard") // Pass the name of the action to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Blizzard", "Deal 5 damage to 2 targets and reduces their attack power."); // Set the action description
        }

        public int MaxTargets => 2;
        public int MinTargets => 1;

        public AttackResolution ResolveAttack( // Override the ResolveAttack method to define the behavior of the Ice Ray action
            Character user,
            List<Character> targets,
            CombatEncounterGameState gameState,
            bool isRedirected = false,
            bool isUnblockable = false)
        {
            // Add validation to ensure that the user, target, and gameState are not null
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (targets == null || targets.Count == 0) throw new ArgumentException("At least one target is required.", nameof(targets));
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            // Verify selected targets is less than the max
            if (targets.Count > MaxTargets)
            {
                throw new ArgumentException($"Blizzard can target at most {MaxTargets} targets.", nameof(targets));
            }

            // Validate that the target(s) is/are eligible for this attack
            var eligibleTargets = ReturnEligibleTargets(user, gameState);
            foreach (var target in targets)
            {
                if (!eligibleTargets.Any(c => c.Id == target.Id))
                {
                    throw new ArgumentException($"Target {target.Id} is not eligible for this attack.", nameof(targets));
                }
            }

            // Verify distinct targets are being chosen
            var distinctTargets = targets.GroupBy(t => t.Id).Select(g => g.First()).ToList();

            int hitCount = 0;
            if(targets.Count == 1) { hitCount = 1; }
            else { hitCount = targets.Count; }

            int blizzardDamage = 5;
            var resolution = new AttackResolution(); // Create a new AttackResolution object to hold the results of the attack
            for (int i = 0; i < hitCount; i++)
            {
                var target = distinctTargets[i % distinctTargets.Count];
                resolution.AttackInstances.Add(new AttackInstance 
                {
                    ActionName = AttackName,
                    BaseDamage = blizzardDamage,
                    FinalDamage = blizzardDamage,
                    SourceCharacterId = user.Id,
                    TargetCharacterId = target.Id,
                    IsRedirected = isRedirected,
                    IsUnblockable = isUnblockable,
                });
                resolution.StatusEffectsToApply.Add(new ChillStatus(user, target)); // Add a new ChillStatus effect to the list of status effects to apply to the target
                resolution.Events.Add(new CombatEvent("chill_status_applied", new StatusAppliedEventParams { OwnerId = user.Id })); // Add a new combat event to indicate that the chill status has been applied
            }
            return resolution;
        }

        public override AttackResolution ResolveAttack(Character user, Character target, CombatEncounterGameState gameState, bool isRedirected = false, bool isUnblockable = false)
        {
            return ResolveAttack(user, new List<Character> { target }, gameState, isRedirected, isUnblockable);
        }
    }
}
