using GnomeParty.Database;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.EncounterData;
using Models.Status;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace CombatService.Tests;

public class ExtraActionsCSTests
{
    private readonly ITestOutputHelper testOutputHelper;
    public ExtraActionsCSTests(ITestOutputHelper testOutputHelper) => this.testOutputHelper = testOutputHelper;

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
    public async Task FuryStrikesThrowsWhenTargetIsAlly()
    {
        var player = new Warrior("player");
        var player2 = new Warrior("player2");
        var enemy = new Skeleton { Id = "enemy1", Health = 20, MaxHealth = 20 };

        var encounter = new ActiveCombatEncounter(
            new List<Character> { player, player2 },
            new List<Character> { enemy });

        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object, new TestRandomGenerator(0.0));

        var results1 = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = player.Id,
            TargetCharacterId = enemy.Id,
            Action = "Fury Strikes"
        });

        Assert.Empty(results1);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = player2.Id,
                TargetCharacterId = player.Id,
                Action = "Fury Strikes"
            });
        });
    }

}