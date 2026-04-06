using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Actions
{
    // Fury Strikes: Hit the same target 2 to 4 times for 3 damage each
    public sealed class FuryStrikes : CharacterAction
    {
        private readonly int hitCount;
        public FuryStrikes() : this(Random.Shared.Next(2, 5)) { } // Default to random hit count between 2 and 4
        public FuryStrikes(int hitCount) : base("Fury Strikes") // Allow specifying hit count for testing or specific scenarios
        {
            if (hitCount < 2 || hitCount > 4) // Validate hit count is within allowed range
            {
                throw new ArgumentOutOfRangeException(nameof(hitCount));
            }
            this.hitCount = hitCount; // Store hit count for use in attack resolution
            ActionDescription = new CharacterActionDescription( // Set action description for Fury Strikes
                "Fury Strikes",
                "Hit the same target 2 to 4 times for 3 damage each"
            );
        }
        public override AttackResolution ResolveAttack( // Override ResolveAttack to implement Fury Strikes logic
            Character user, 
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected = false, 
            bool isUnblockable = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user)); // Validate user is not null
            if (target == null) throw new ArgumentNullException(nameof(target)); // Validate target is not null

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, target, gameState);
            if(!eligibleTargets.Contains(target)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target));}

            var resolution = new AttackResolution(); // Create new AttackResolution to hold the results of the attack
            for (int i = 0; i < hitCount; i++) // Loop through the number of hits and add an AttackInstance for each hit
            {
                resolution.AttackInstances.Add(new AttackInstance
                {
                    ActionName = AttackName,
                    BaseDamage = 3,
                    FinalDamage = 3,
                    SourceCharacterId = user.Id,
                    TargetCharacterId = target.Id,
                });
            }
            return resolution;
        }

        // Override the ReturnEligibleTargets method to return all members of the opposing team as eligible targets
        public override List<Character> ReturnEligibleTargets(Character user, Character target, CombatEncounterGameState gameState)
        {
            List<Character> eligibleTargets = new List<Character>();
            eligibleTargets = TargetingService.GetOpposingTeam(gameState, user.Id);
            return eligibleTargets;
        }
    }
}
