using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text;
using GnomeParty.Database;
using Models.CharacterData;
using Models.CharacterData.BossEnemyPoolClasses;
using Models.CharacterData.DifficultEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.EncounterData;
using Moq;
using Xunit;
using Xunit.Abstractions;
using static System.Net.Mime.MediaTypeNames;

namespace CombatService.Tests.DifficultEnemyPoolCSTests
{
    public class GnombieBruteCSTests
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
        /*******************************************************************************************************************/

        [Theory]
        // Test: Use Rotting Aura on successful roll and weaken Bard
        // Rotting Aura Roll: 0.4 (Needs 50% or less. Should succeed.)
        // Targeting Roll: 0.0
        // Turn Count: 3
        [InlineData(0.4, 0.0, 3)]
        public async Task ValidateHeavySlamAndRottingAura(double rottingAuraRoll, double targetingRoll, int turnCount)
        {
            // Initialize random variables
            var rng = new TestRandomGenerator(rottingAuraRoll, targetingRoll, 0.8, targetingRoll);

            // Initialize character for testing
            var mage = new Mage("mage") { Health = 30, MaxHealth = 30 };
            var gnombieBrute = new GnombieBrute() { Id = "gnombieBrute", Health = 30, MaxHealth = 30 };

            // Create a combat encounter with the players and enemy
            var encounter = new ActiveCombatEncounter(new List<Character> { mage }, new List<Character> { gnombieBrute });

            // Initialize mock database and service to test combat service
            var mockDb = BuildDbMock(encounter);
            var service = new CombatService(mockDb.Object, rng);

            gnombieBrute.turnCount = turnCount;

            // -------------------------
            // ROUND 1
            // -------------------------

            // Create combat requests for Mage Round 1 action
            var resultsM1 = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = mage.Id,
                TargetCharacterId = gnombieBrute.Id,
                Action = "Magic Missile"
            });
            Assert.NotEmpty(resultsM1); // Verify a mage action was taken
            Assert.Equal(20, gnombieBrute.Health); // Verify Magic Missile hit the Gnombie Brute
            Assert.Equal(30, mage.Health); // Verify the mage received no damage (Rotten Aura shouldn't do damage)

            ResetEncounterForNextRound(encounter);

            // -------------------------
            // ROUND 2
            // -------------------------

            // Create combat requests for Mage Round 2 action
            var resultsM2 = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = mage.Id,
                TargetCharacterId = gnombieBrute.Id,
                Action = "Magic Missile"
            });
            Assert.NotEmpty(resultsM2); // Verify a mage action was taken
            Assert.Equal(10, gnombieBrute.Health); // Verify Magic Missile hit the Gnombie Brute
            Assert.Equal(16, mage.Health); // Verify the mage received 14 damage from Heavy Slam instead of 12

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
