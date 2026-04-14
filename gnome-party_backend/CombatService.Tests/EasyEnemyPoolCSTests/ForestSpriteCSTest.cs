using System;
using System.Collections.Generic;
using System.Text;
using GnomeParty.Database;
using Models;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.EncounterData;
using Moq;
using Xunit;
using static System.Net.Mime.MediaTypeNames;

namespace CombatService.Tests.EasyEnemyPoolCSTests
{
    public class ForestSpriteCSTest
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
        /* Test: Use Entangle on a Bard
        // Entangle Chance Roll: 0.45 (Needs a <= 60% to succeed. Should succeed)
        // Targeting Roll: 0.33 (Should return Bard)
        //      * 0 - 49%: Bard
        //      * 50 - 79%: Mage
        //      * 80 - 100% Warrior  
        // Target Tie breaking roll: 0.0
        // 2nd Target Tie breaking roll: 0.0       
        */
        [InlineData(0.45, 0.33, 0.0, 0.0)]
        public async Task EntangleChoosesBardAndStunsOnNextTurn(
        double entangleRoll,
        double targetingRoll,
        double tieBreakerRoll,
        double secondTieBreakerRoll)
        {
            var rng = new TestRandomGenerator(
                entangleRoll, 
                targetingRoll, 
                tieBreakerRoll, 
                secondTieBreakerRoll, 
                entangleRoll,
                targetingRoll,
                tieBreakerRoll,
                secondTieBreakerRoll); // Simulate random numbers for the test

            // Create characters for testing
            var forestSprite = new ForestSprite() { Id = "forestSprite", Health = 20, MaxHealth = 20 };
            var warrior = new Warrior("warrior") { Health = 20, MaxHealth = 20 };
            var bard = new Bard("bard") { Health = 20, MaxHealth = 20 };
            var enemies = new List<Character> { bard, warrior };
            var allies = new List<Character> { forestSprite };

            // Manually insert the Forest Sprite actions
            var actions = new List<string> { "Leaf Dart", "Entangle" };

            // Create an encounter with the characters
            var encounter = new ActiveCombatEncounter( enemies, allies );

            // Create a mockdb and service for testing
            var mockDb = BuildDbMock(encounter);
            var service = new CombatService(mockDb.Object, rng);

            // -------------------------
            // ROUND 1
            // -------------------------

            // Create combat requests for round 1 actions
            var warriorResultsR1 = await service.CombatRequestHandlerAsync(new CombatRequest 
            {
                EncounterId = "game1",
                SourceCharacterId = warrior.Id,
                TargetCharacterId = forestSprite.Id,
                Action = "Whirling Strike",
            });
            Assert.Empty( warriorResultsR1 ); // Should be null until both player characters have acted
            var bardResultsR1 = await service.CombatRequestHandlerAsync(new CombatRequest 
            {
                SourceCharacterId = bard.Id,
                TargetCharacterId = warrior.Id,
                Action = "Song",
            });
            Assert.NotEmpty( bardResultsR1 ); // SHould NOT be null since both player characters have acted

            // Verify correct health after round 1 actions
            Assert.Equal(15, forestSprite.Health);
            Assert.Equal(20, bard.Health);
            Assert.Equal(20, warrior.Health);

            ResetEncounterForNextRound(encounter);

            // -------------------------
            // ROUND 2
            // -------------------------

            // Create combat requests for round 1 actions
            var warriorResultsR2 = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = "game1",
                SourceCharacterId = warrior.Id,
                TargetCharacterId = forestSprite.Id,
                Action = "Whirling Strike",
            });
            Assert.Empty(warriorResultsR2); // Should be null until both player characters have acted
            var bardResultsR2 = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                SourceCharacterId = bard.Id,
                TargetCharacterId = warrior.Id,
                Action = "Song",
            });
            Assert.NotEmpty(bardResultsR2); // SHould NOT be null since both player characters have acted

            // Verify correct health after round 1 actions
            Assert.Equal(10, forestSprite.Health);
            Assert.Equal(20, bard.Health);
            Assert.Equal(20, warrior.Health);

            var damageResults = bardResultsR2.FirstOrDefault(r => r.Request.SourceCharacterId == bard.Id);
            Assert.NotNull(damageResults);
            Assert.Contains(damageResults!.Events, e => e.Event == "stun_expired");
            Assert.DoesNotContain(damageResults.Events, e => e.Event == "damage");
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
