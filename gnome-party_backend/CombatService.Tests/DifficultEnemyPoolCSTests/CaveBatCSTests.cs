using System;
using System.Collections.Generic;
using System.Text;
using Models.TestHelperData;
using CombatService.Tests;
using GnomeParty.Database;
using Models.CharacterData;
using Models.CharacterData.DifficultEnemyPoolClasses;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.EncounterData;
using Moq;
using Xunit;

namespace CombatService.Tests.DifficultEnemyPoolCSTests
{
    public  class CaveBatCSTests
    {
        /*******************************************************************************************************************/
        //this test is just for debugging and should be replaced with more specific tests
        [Fact]
        public async Task TestCombatRequestHandlerAsync()
        {
            // Arrange
            var encounter = new ActiveCombatEncounter(
                new List<Character>
                {
                new Character("test-source-character-id"),
                new Character("test-source-character-id-2")
                },
                new List<Character>
                {
                new Skeleton { Id = "test-target-character-id" },
                new Skeleton()
                }
            );

            var mockDBService = new Mock<IDatabaseService>();
            mockDBService
                .Setup(dbService => dbService.LoadAsync<ActiveCombatEncounter>(It.IsAny<object>()))
                .ReturnsAsync(encounter);

            var combatService = new CombatService(mockDBService.Object, new TestRandomGenerator(0.0));

            // Act
            var result1 = await combatService.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = "test-encounter-id",
                SourceCharacterId = "test-source-character-id",
                TargetCharacterId = "test-target-character-id",
                Action = "Slash",
            });

            var result2 = await combatService.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = "test-encounter-id",
                SourceCharacterId = "test-source-character-id-2",
                TargetCharacterId = "test-target-character-id",
                Action = "Slash",
            });

            // Assert
            Assert.NotNull(result1);
            Assert.IsType<List<CombatResult>>(result1);
        }

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

        [Fact]
        // Test: Sonic Squeal correctly damages all enemies
        public async Task SonicSquealDamagesAllEnemies()
        {
            // Initialize Cave Bat and Player team
            var caveBat = new CaveBat { Id = "caveBat", Health = 50, MaxHealth = 50 };
            var enemy1 = new Warrior("player1") { Health = 10, MaxHealth = 10 };
            var enemy2 = new Warrior("player2") { Health = 10, MaxHealth = 10 };
            var enemy3 = new Warrior("player3") { Health = 10, MaxHealth = 10 };
            var enemy4 = new Warrior("player4") { Health = 10, MaxHealth = 10 };

            var playerEnemies = new List<Character> { enemy1, enemy2, enemy3, enemy4 };

            // Create a combat encounter with the cave bat and enemy team
            var encounter = new ActiveCombatEncounter(playerEnemies, new List<Character> { caveBat });

            // Initialize mock database and service to test combat service
            var mockDb = BuildDbMock(encounter);
            var service = new CombatService(mockDb.Object, new TestRandomGenerator(0.0,0.0));

            // Execute the player characters attacks
            var result1 = await service.CombatRequestHandlerAsync(new CombatRequest 
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = enemy1.Id,
                TargetCharacterId = caveBat.Id,
                Action = "Slash"
            });
            Assert.Empty(result1);

            var result2 = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = enemy2.Id,
                TargetCharacterId = caveBat.Id,
                Action = "Slash"
            });
            Assert.Empty(result2);

            var result3 = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = enemy3.Id,
                TargetCharacterId = caveBat.Id,
                Action = "Slash"
            });
            Assert.Empty(result3);

            var result4 = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = enemy4.Id,
                TargetCharacterId = caveBat.Id,
                Action = "Slash"
            });
            Assert.NotEmpty(result4); // Verify that no result was passed until everyone has readied up

            var playerResult = result4.First(r => r.Request.Action == "Sonic Squeal" && r.Request.SourceCharacterId == caveBat.Id); // Find the result for Sonic Squeal

            // Loop through each player character to see if the damage was calculated correctly
            foreach(var enemy in playerEnemies)
            {
                Assert.Equal(7, enemy.Health); // each player character should take 3 damage
            }

            Assert.Equal(10, caveBat.Health); // Cave Bat should have 10 health left after receiving 4 slash attacks
        }

        [Fact]
        // Test: Blood Peck deals correct damage and heals user for the correct amount
        public async Task BloodPeckDealsDamageAndHealsInCombatService()
        {
            // Initialize cave bat and target for testing
            var caveBat = new CaveBat { Id = "caveBat", Health = 17, MaxHealth = 20 };
            var target = new Mage("player1") { Health = 6, MaxHealth = 30 };

            // Create a combat encounter with the cave bat and target
            var encounter = new ActiveCombatEncounter(new List<Character> { target }, new List<Character> { caveBat });

            // Initialize mock database and service to test combat service
            var mockDb = BuildDbMock(encounter);
            var service = new CombatService(mockDb.Object, new TestRandomGenerator(0.7, 0.0));

            // Execute player character action
            var results = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = target.Id,
                TargetCharacterId = caveBat.Id,
                Action = "Slash"
            });

            Assert.NotEmpty(results); // Verify player action was passed

            // Verify Blood Peck was done
            var batResult = results.FirstOrDefault(r => r.Request.SourceCharacterId == caveBat.Id && r.Request.Action == "Blood Peck");
            Assert.NotNull(batResult);

            // Verfiy the attack / damage instance from Blood Peck
            var damageEvent = batResult!.Events.FirstOrDefault(e => e.Event == "damage");
            Assert.NotNull(damageEvent);
            var damageParams = Assert.IsType<DamageEventParams>(damageEvent!.Params);
            Assert.Equal(caveBat.Id, damageParams.SourceId);
            Assert.Equal(target.Id, damageParams.TargetId);
            Assert.Equal(5, damageParams.DamageAmount);

            // Verify the healing instance from Blood Peck
            var healEvent = batResult.Events.FirstOrDefault(e => e.Event == "healed");
            Assert.NotNull(healEvent);
            var healParams = healEvent!.Params;
            var healSourceId = (string)healParams.GetType().GetProperty("SourceId")!.GetValue(healParams)!;
            var healTargetId = (string)healParams.GetType().GetProperty("TargetId")!.GetValue(healParams)!;
            var healingAmount = (int)healParams.GetType().GetProperty("HealingAmount")!.GetValue(healParams)!;
            Assert.Equal(target.Id, healSourceId);
            Assert.Equal(caveBat.Id, healTargetId);
            Assert.Equal(3, healingAmount);

            // Final health checks after CombatService applies effects
            Assert.Equal(1, target.Health); // Took 5 damage from Blood Peck (6 - 5 = 1)
            Assert.Equal(10, caveBat.Health); // Healed for +3, but took -10 from the Warrior's Slash (17 + 3 = 20 - 10 = 10)
        }
    }
}
