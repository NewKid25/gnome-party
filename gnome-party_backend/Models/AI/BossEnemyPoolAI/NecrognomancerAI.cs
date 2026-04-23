using Models.CharacterData;
using Models.CharacterData.BossEnemyPoolClasses;
using Models.CombatData;
using Models.TestHelperData;

namespace Models.AI.BossEnemyPoolAI
{
    internal class NecrognomancerAI : CharacterAI
    {
        public NecrognomancerAI() { }
        public NecrognomancerAI(IRandomGenerator rng) : base(rng) { }
        public override CombatRequest ChooseAction(Character self, List<string> actions, List<Character> enemies, List<Character> allies, List<CombatRequest> playerRequests)
        {
            // Defensive check to ensure we have a reference to ourself
            if (self == null) { throw new ArgumentException("Reference to self cannot be null"); }

            // Defensive check to ensure we have actions to choose from
            if (actions == null || actions.Count == 0) { throw new ArgumentException("Actions list cannot be null or empty."); }

            // Defensive check to ensure we have enemies to target
            if (enemies == null || enemies.Count == 0) { throw new ArgumentException("Enemies list cannot be null or empty."); }

            var aliveEnemies = enemies.Where(e => e.Health > 0).ToList(); // Filter out dead enemies
            // If there are no alive enemies, we can't target anyone, so we should handle this case appropriately
            if (aliveEnemies.Count == 0) { throw new InvalidOperationException("No alive enemies to target."); }

            // Verify all the Necrognomancer actions are present
            bool hasDarkBolt = actions.Contains("Dark Bolt");
            bool hasSoulDrain = actions.Contains("Soul Drain");
            bool hasSummon = actions.Contains("Summon");

            string chosenAction = null; // Variable to hold the chosen action
            if (self is not Necrognomancer)
            {
                throw new InvalidOperationException($"Expected Necrognomancer but got {self.GetType().Name}");
            }

            int summonMax = 3;
            int soulDrainTurnCount = 3;
            if(hasSummon && (self as Necrognomancer).ActiveSummonCount < summonMax) { chosenAction = "Summon"; }
            else if(hasSoulDrain && (self as Necrognomancer).TurnCount != 0 && (self as Necrognomancer).TurnCount % soulDrainTurnCount == 0) { chosenAction = "Soul Drain"; }
            else if (hasDarkBolt){ chosenAction = "Dark Bolt"; }
            else { chosenAction = GetDefaultAction(actions); }

            bool stunBurstActions = IsStunBurstAction(playerRequests);

            var target = GetLowestHealthTarget(aliveEnemies);

            if(stunBurstActions) { target = GetStunBurstUser(playerRequests, aliveEnemies); }

            return new CombatRequest { Action = GetDefaultAction(actions), TargetCharacterId = GetLowestHealthTarget(aliveEnemies).Id }; 
        }

        public override CombatRequest ChooseAction(Character self, List<string> actions, List<Character> enemies, List<Character> allies)
        {
            return new CombatRequest
            {
                Action = GetDefaultAction(actions),
                TargetCharacterId = GetLowestHealthTarget(enemies).Id
            };
        }
    }
}
