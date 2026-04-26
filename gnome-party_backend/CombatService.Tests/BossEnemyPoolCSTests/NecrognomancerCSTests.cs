using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using GnomeParty.Database;
using Models.Actions.BossPoolActions.NecrognomancerActions;
using Models.CharacterData;
using Models.CharacterData.BossEnemyPoolClasses;
using Models.CharacterData.BossEnemyPoolClasses.Summons;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.EncounterData;
using Models.GameMetaData;
using Moq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace CombatService.Tests.BossEnemyPoolCSTests
{
    public class NecrognomancerCSTests
    {
        /*******************************************************************************************************************/

        // Helper method to build a mock database service that returns the provided encounter when LoadAsync is called
        private static Mock<IDatabaseService> BuildDbMock(ActiveCombatEncounter encounter)
        {
            var mockDb = new Mock<IDatabaseService>(); // Create a new mock of the IDatabaseService interface

            mockDb.Setup(db => db.LoadAsync<ActiveCombatEncounter>(It.IsAny<object>())) // Set up the LoadAsync method to return the provided encounter when called with any object as the hash key
                  .ReturnsAsync(encounter);

            mockDb.Setup(db => db.SaveAsync(It.IsAny<ActiveCombatEncounter>())) // Set up the SaveAsync method to do nothing (just return a completed task) when called with any ActiveCombatEncounter object
                  .Returns(Task.CompletedTask);

            return mockDb; // Return the configured mock database service
        }
        private readonly ITestOutputHelper testOutputHelper;
        public NecrognomancerCSTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }
        private void PrintFrontendPayload(List<CombatResult> response)
        {
            var payload = new ConnectionMessage("action-handler", response);

            var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            testOutputHelper.WriteLine("=== FRONTEND PAYLOAD ===");
            testOutputHelper.WriteLine(payloadJson);
            testOutputHelper.WriteLine("=== END FRONTEND PAYLOAD ===");
        }
        /*******************************************************************************************************************/

        [Theory]
        // Test: Uses Summon 2 in a row due to only 1 summon being present
        [InlineData(0.0, 0.0)]
        public async Task VerifyNecrognomancerAttacks(double tiebreaker, double secondTieBreaker)
        {
            var rng = new TestRandomGenerator(tiebreaker, secondTieBreaker, tiebreaker, secondTieBreaker, tiebreaker, secondTieBreaker); // Random numbers being manually inserted
            
            //Initialize characters for testing
            var necrognomancer = new Necrognomancer() { Id = "Necrognomancer", Health = 100, MaxHealth = 100 };
            var warrior = new Warrior("warrior") { Health = 100, MaxHealth = 100 };
            var alliedEnemies = new List<Character> { necrognomancer };
            var playerCharacters = new List<Character> { warrior };

            // Create combat encounter for testing
            var encounter = new ActiveCombatEncounter(playerCharacters, alliedEnemies);
            var mockDb = BuildDbMock(encounter);
            var service = new CombatService(mockDb.Object, rng);

            testOutputHelper.WriteLine("CURRENT !!! TURN !!! COUNT: " + necrognomancer.TurnCount);

            // -------------------------
            // ROUND 1
            // -------------------------

            // Have warrior do slash
            var resultsR1 = await service.CombatRequestHandlerAsync(new CombatRequest 
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = warrior.Id,
                TargetCharacterId = necrognomancer.Id,
                Action = "Slash"
            });

            Assert.NotEmpty(resultsR1); // Verify results were passed
            PrintFrontendPayload(resultsR1);

            Assert.Equal(100, warrior.Health); // Shouldn't have been attacked this turn
            Assert.Equal(90, necrognomancer.Health); // Should have taken 10 damage from Slash
            Assert.Equal(3, necrognomancer.ActiveSummonCount); // Started with 3 summons
            Assert.Equal(4, encounter.GameState.EnemyCharacters.Count()); // Should have 3 newly added enemies

            ResetEncounterForNextRound(encounter);

            testOutputHelper.WriteLine("CURRENT !!! TURN !!! COUNT: " + necrognomancer.TurnCount);

            // -------------------------
            // ROUND 2
            // -------------------------

            var skeleton2 = encounter.GameState.EnemyCharacters.First(c => c.Name == "Summoned Skeleton 2");

            var resultsR2 = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = warrior.Id,
                TargetCharacterId = skeleton2.Id,
                Action = "Slash"
            });

            Assert.NotEmpty(resultsR2); // Verify results were passed
            PrintFrontendPayload(resultsR2);

            Assert.Equal(78, warrior.Health); // Should have been attacked by 2 Bone Slash attacks (for 12 damage) and 1 Dark Bolt (for 10 damage) = 22 damage total
            Assert.Equal(90, necrognomancer.Health); // Shouldn't have been targeted this turn
            Assert.Equal(2, necrognomancer.ActiveSummonCount); // Should have lost a summon
            Assert.Equal(3, encounter.GameState.EnemyCharacters.Count()); // Should have lost an enemy

            ResetEncounterForNextRound(encounter);

            // -------------------------
            // ROUND 3
            // -------------------------

            testOutputHelper.WriteLine("CURRENT !!! TURN !!! COUNT: " + necrognomancer.TurnCount);

            var resultsR3 = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = warrior.Id,
                TargetCharacterId = necrognomancer.Id,
                Action = "Slash"
            });

            Assert.NotEmpty(resultsR3); // Verify results were passed
            PrintFrontendPayload(resultsR3);

            Assert.Equal(60, warrior.Health); // Should have been attacked by 2 Bone Slash attacks (for 12 damage) and 1 Soul Drain (for 6 damage) = 18 damage total
            Assert.Equal(86, necrognomancer.Health); // Should have been hit for 10 damage from Warrior's Slash, but healed for 6.
            Assert.Equal(2, necrognomancer.ActiveSummonCount); // Should have the same amount of summmons
            Assert.Equal(3, encounter.GameState.EnemyCharacters.Count()); // Should have the same amount of enemies
        }

        private static void ResetEncounterForNextRound(ActiveCombatEncounter encounter)
        {
            if (encounter == null)
            {
                throw new ArgumentNullException(nameof(encounter));
            }

            for (int i = 0; i < encounter.PlayerReadied.Count; i++)
            {
                encounter.PlayerReadied[i] = false;
            }

            for (int i = 0; i < encounter.CombatRequests.Count; i++)
            {
                encounter.CombatRequests[i] = null;
            }
        }

    }
}
