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

    [Fact]
    // Test : Verifying that GetActionTargets returns eligible targets for each action when called within the Combat Service
    public void GetActionTargetReturnsEligibleTargetsForEachAction()
    {
        var warrior = new Warrior("warrior");
        var ally = new Warrior("ally");
        var ally2 = new Warrior("ally2");
        var enemy1 = new Skeleton { Id = "enemy1", Health = 20, MaxHealth = 20 };
        var enemy2 = new Skeleton { Id = "enemy2", Health = 20, MaxHealth = 20 };
        var enemy3 = new Skeleton { Id = "enemy3", Health = 20, MaxHealth = 20 };


        var encounter = new ActiveCombatEncounter(
            new List<Character> { warrior, ally, ally2 },
            new List<Character> { enemy1, enemy2, enemy3 });

        var mockDb = new Mock<IDatabaseService>();
        mockDb.Setup(db => db.LoadAsync<ActiveCombatEncounter>(encounter.EncounterId))
              .ReturnsAsync(encounter);

        var service = new CombatService(mockDb.Object, new TestRandomGenerator(0.0));

        var result = service.GetActionTargets(encounter.EncounterId, warrior.Id);

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var slashInfo = result.First(x => x.ActionName == "Slash");
        Assert.NotNull(slashInfo);
        var slashids = slashInfo.EligibleTarget.Select(t => t.CharacterId).ToList();
        testOutputHelper.WriteLine($"Slash Targets: {string.Join(", ", slashids)}");

        Assert.Equal(3, slashInfo!.EligibleTarget.Count);
        Assert.Contains(slashInfo.EligibleTarget, t => t.CharacterId == enemy1.Id);
        Assert.Contains(slashInfo.EligibleTarget, t => t.CharacterId == enemy2.Id);

        var blockInfo = result.First(x => x.ActionName == "Block");
        Assert.NotNull(blockInfo);
        var blockids = blockInfo.EligibleTarget.Select(t => t.CharacterId).ToList();
        testOutputHelper.WriteLine($"Block Targets: {string.Join(", ", blockids)}");

        Assert.Contains(blockInfo!.EligibleTarget, t => t.CharacterId == warrior.Id);
        Assert.Contains(blockInfo.EligibleTarget, t => t.CharacterId == ally.Id);
        Assert.Contains(blockInfo.EligibleTarget, t => t.CharacterId == ally2.Id);

        var parryInfo = result.First(x => x.ActionName == "Parry");
        Assert.NotNull(parryInfo);
        var parryids = parryInfo.EligibleTarget.Select(t => t.CharacterId).ToList();
        testOutputHelper.WriteLine($"Parry Targets: {string.Join(", ", parryids)}");

        Assert.Contains(parryInfo.EligibleTarget, t => t.CharacterId == enemy1.Id);
        Assert.Contains(parryInfo.EligibleTarget, t => t.CharacterId == enemy2.Id);
    }

}