using Models.ActionMetaData;
using Models.CharacterData;
using Models.CharacterData.BossEnemyPoolClasses;
using Models.CharacterData.BossEnemyPoolClasses.Summons;
using Models.CombatData;

namespace Models.Actions.BossPoolActions.NecrognomancerActions
{
    // Summon: Summon an allied monster (Weakened Skeleton right now)
    public sealed class Summon : CharacterAction
    {
        private static readonly SummonType[] SummonTypes =
        {
            SummonType.Skeleton,
            SummonType.ForestSprite,
            SummonType.GoblinArcher
        };
        private static SummonType GetWeightedSummonType()
        {
            double roll = Random.Shared.NextDouble();
            double runningTotal = 0.0;

            foreach (var entry in SummonTypeData.WeightedSummons)
            {
                runningTotal += entry.Weight;
                if (roll <= runningTotal)
                {
                    return entry.Type;
                }
            }

            return SummonTypeData.WeightedSummons[^1].Type;
        }
        public Summon() : base("Summon") // Call the base constructor with the name of the action
        {
            ActionDescription = new CharacterActionDescription("Summon", "Summon a weakened monster as an ally"); // Set the action description
        }
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

            if (user is not Necrognomancer) { throw new InvalidOperationException("Summon can only be used by the Necrognomancer."); }

            var resolution = new AttackResolution(); // Create a new AttackResolution object to hold the results of the attack

            // Maxmimum of 3 summons
            int currentSummonCount = gameState.EnemyCharacters.Count(c => c is Summons);
            int maxSummons = 3;
            if(currentSummonCount > maxSummons)
            {
                resolution.Events.Add(new CombatEvent("summon_failed", new
                {
                    sourceId = user.Id,
                    reason = "max_summons_reached"
                }));
                return resolution;
            }

            // Summon based on a random (but weighted) summon
            SummonType selectedType = GetWeightedSummonType();
            var summonedUnit = new Summons(selectedType);

            resolution.SummonedCharacters.Add(summonedUnit);
            resolution.Events.Add(new CombatEvent("summoned", new
            {
                sourceId = user.Id,
                summonId = summonedUnit.Id,
                summonType = summonedUnit.CharacterType,
                summonName = summonedUnit.Name
            }));
            return resolution;
        }
    }
}
