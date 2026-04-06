using GnomeParty.Database;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.EncounterData;
using Models.Status;
using Moq;
using Xunit;

namespace CombatService.Tests;

public class ExtraActionsCSTests
{
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

        var combatService = new CombatService(mockDBService.Object);

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

    [Fact]
    // Test: Fury Strikes produces multiple damage events
    public async Task FuryStrikesProducesMultipleDamageEvents()
    {
        // Initialize player and enemy for testing
        var player = new Warrior("player");
        var enemy = new Skeleton { Id = "enemy1", Health = 20, MaxHealth = 20 };

        // Create combat encounter and service
        var encounter = new ActiveCombatEncounter(
            new List<Character> { player },
            new List<Character> { enemy });

        // Initialize mockdb and service for testing
        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        // Player uses Fury Strikes, which should produce 2-4 damage events against the enemy
        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = player.Id,
            TargetCharacterId = enemy.Id,
            Action = "Fury Strikes"
        });

        Assert.NotEmpty(results); // Verify that results were passed from Combat Request

        // Verify that the correct results were passed
        var playerResult = results.First(r =>
            r.Request.Action == "Fury Strikes" &&
            r.Request.SourceCharacterId == player.Id);

        var damageEvents = playerResult.Events.Where(e => e.Event == "damage").ToList();

        // Verify that the damage produced was within range
        Assert.InRange(damageEvents.Count, 2, 4);
        Assert.InRange(enemy.Health, 8, 14);
    }

}