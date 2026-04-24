using Models.ActionMetaData;
using Models.CharacterData;
using Models.CharacterData.BossEnemyPoolClasses;
using Models.CharacterData.BossEnemyPoolClasses.Summons;
using Models.CombatData;
using Models.TestHelperData;

namespace Models.Actions.BossPoolActions.NecrognomancerActions
{
    // Summon: Summon an allied monster (Weakened Skeleton right now)
    public sealed class Summon : CharacterAction
    {
        private readonly IRandomGenerator rng;
        public Summon() : this(new RandomNumGen()) { }
        public Summon(IRandomGenerator rng) : base("Summon")
        {
            if (rng == null) throw new ArgumentNullException(nameof(rng));
            this.rng = rng;
            ActionDescription = new CharacterActionDescription("Summon", "Summon a weakened monster as an ally");
        }
        private static readonly SummonType[] SummonTypes =
        {
            SummonType.Skeleton,
            SummonType.ForestSprite,
            SummonType.GoblinArcher
        };
        private SummonType GetWeightedSummonType()
        {
            double roll = rng.NextDouble();
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
        private static string GetSummonName(CombatEncounterGameState gameState, SummonType summonType, List<Character>? pendingSummons = null)
        {
            string defaultName = summonType switch
            {
                SummonType.Skeleton => "Summoned Skeleton",
                SummonType.ForestSprite => "Summoned Forest Sprite",
                SummonType.GoblinArcher => "Summoned Goblin Archer",
                _ => throw new ArgumentOutOfRangeException(nameof(summonType), summonType, null)
            };

            var existingSummons = gameState.EnemyCharacters.OfType<Summons>();
            var pending = pendingSummons?.OfType<Summons>() ?? Enumerable.Empty<Summons>();

            var usedNumbers = existingSummons
                .Concat(pending)
                .Where(s => s.Name.StartsWith(defaultName + " "))
                .Select(s =>
                {
                    string suffix = s.Name[(defaultName.Length + 1)..];
                    return int.TryParse(suffix, out int number) ? number : 0;
                })
                .Where(n => n > 0)
                .ToHashSet();

            int nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return $"{defaultName} {nextNumber}";
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
            if(currentSummonCount >= maxSummons)
            {
                resolution.Events.Add(new CombatEvent("summon_failed", new
                {
                    sourceId = user.Id,
                    reason = "max_summons_reached"
                }));
                return resolution;
            }
            if (user is Necrognomancer necro)
            {
                if (necro.TurnCount == 1)
                {
                    var pendingSummons = new List<Character>();
                    for (int i = currentSummonCount; i < maxSummons; i++)
                    {
                        SummonType selectedType = GetWeightedSummonType();

                        var summonedUnit = new Summons(selectedType)
                        {
                            Name = GetSummonName(gameState, selectedType, pendingSummons)
                        };

                        pendingSummons.Add(summonedUnit);
                        resolution.SummonedCharacters.Add(summonedUnit);

                        resolution.Events.Add(new CombatEvent("summoned", new SummonedEventParams
                        {
                            SourceId = user.Id,
                            SummonId = summonedUnit.Id,
                            SummonType = summonedUnit.CharacterType,
                            SummonName = summonedUnit.Name
                        }));
                    }
                }
                else
                {
                    // Summon based on a random (but weighted) summon
                    SummonType selectedType = GetWeightedSummonType();
                    var summonedUnit = new Summons(selectedType) { Name = GetSummonName(gameState, selectedType) };

                    resolution.SummonedCharacters.Add(summonedUnit);
                    resolution.Events.Add(new CombatEvent("summoned", new SummonedEventParams
                    {
                        SourceId = user.Id,
                        SummonId = summonedUnit.Id,
                        SummonType = summonedUnit.CharacterType,
                        SummonName = summonedUnit.Name
                    }));
                }
            }
            return resolution;
        }
    }
}
