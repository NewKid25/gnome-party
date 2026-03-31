using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using Amazon.Lambda.TestUtilities;
using CombatService;
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

public class CombatServiceTests
{
    public CombatServiceTests()
    {

    }
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

    private static Mock<IDatabaseService> BuildDbMock(ActiveCombatEncounter encounter)
    {
        var mockDb = new Mock<IDatabaseService>();

        mockDb.Setup(db => db.LoadAsync<ActiveCombatEncounter>(It.IsAny<object>()))
              .ReturnsAsync(encounter);

        mockDb.Setup(db => db.SaveAsync(It.IsAny<ActiveCombatEncounter>()))
              .Returns(Task.CompletedTask);

        return mockDb;
    }
    [Fact]
    public async Task Slash_WithTwoPlayers_ProcessesAfterBothReady_AndProducesDamageEvent()
    {
        var player1 = new Warrior("player1");
        var player2 = new Warrior("player2");
        var enemy1 = new Skeleton { Id = "enemy1", Health = 20, MaxHealth = 20 };
        var enemy2 = new Skeleton { Id = "enemy2", Health = 20, MaxHealth = 20 };

        var encounter = new ActiveCombatEncounter(
            new List<Character> { player1, player2 },
            new List<Character> { enemy1, enemy2 });

        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        var result1 = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = player1.Id,
            TargetCharacterId = enemy1.Id,
            Action = "Slash"
        });

        Assert.Empty(result1);

        var result2 = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = player2.Id,
            TargetCharacterId = enemy1.Id,
            Action = "Slash"
        });

        Assert.NotEmpty(result2);

        var playerSlashResults = result2
            .Where(r => r.Request.Action == "Slash" &&
                        (r.Request.SourceCharacterId == player1.Id || r.Request.SourceCharacterId == player2.Id))
            .ToList();

        Assert.Equal(2, playerSlashResults.Count);

        var damageEvents = playerSlashResults
            .SelectMany(r => r.Events)
            .Where(e => e.Event == "damage")
            .ToList();

        Assert.Equal(2, damageEvents.Count);
        Assert.Equal(0, enemy1.Health);
    }

    [Fact]
    public async Task BoneSlash_WithOnePlayer_Deals6Damage()
    {
        var player = new Warrior("player");
        var enemy = new Skeleton { Id = "enemy1", Health = 20, MaxHealth = 20 };

        var encounter = new ActiveCombatEncounter(
            new List<Character> { player },
            new List<Character> { enemy });

        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = player.Id,
            TargetCharacterId = enemy.Id,
            Action = "Bone Slash"
        });

        Assert.NotEmpty(results);

        var playerResult = results.First(r =>
            r.Request.Action == "Bone Slash" &&
            r.Request.SourceCharacterId == player.Id);

        var damageEvent = playerResult.Events.First(e => e.Event == "damage");
        var damageParams = Assert.IsType<DamageEventParams>(damageEvent.Params);

        Assert.Equal(enemy.Id, damageParams.TargetId);
        Assert.Equal(6, damageParams.DamageAmount);
        Assert.Equal(14, enemy.Health);
    }

    [Fact]
    public async Task Block_RedirectsEnemyAttack_ToBlocker_AndReducesDamage()
    {
        var ally = new Warrior("ally") { Health = 30, MaxHealth = 30 };
        var blocker = new Warrior("blocker") { Health = 30, MaxHealth = 30 };
        var enemy = new Skeleton { Id = "enemy1", Health = 20, MaxHealth = 20 };

        var encounter = new ActiveCombatEncounter(
            new List<Character> { ally, blocker },
            new List<Character> { enemy });

        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        var firstResult = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = ally.Id,
            TargetCharacterId = enemy.Id,
            Action = "Slash"
        });

        Assert.Empty(firstResult);

        var secondResult = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = blocker.Id,
            TargetCharacterId = ally.Id,
            Action = "Block"
        });

        Assert.NotEmpty(secondResult);

        Assert.Equal(30, ally.Health);
        Assert.Equal(27, blocker.Health);

        var enemyDamageResult = secondResult.FirstOrDefault(r => r.Request.SourceCharacterId == enemy.Id);
        Assert.NotNull(enemyDamageResult);
        Assert.Contains(enemyDamageResult!.Events, e => e.Event == "damage");
    }

    [Fact]
    public async Task Fireball_NormalCast_AppliesSplashBurn()
    {
        var caster = new Warrior("caster");
        var enemy1 = new Skeleton { Id = "enemy1", Health = 20, MaxHealth = 20 };
        var enemy2 = new Skeleton { Id = "enemy2", Health = 20, MaxHealth = 20 };
        var enemy3 = new Skeleton { Id = "enemy3", Health = 20, MaxHealth = 20 };

        var encounter = new ActiveCombatEncounter(
            new List<Character> { caster },
            new List<Character> { enemy1, enemy2, enemy3 });

        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = caster.Id,
            TargetCharacterId = enemy2.Id,
            Action = "Fireball"
        });

        Assert.NotEmpty(results);

        var playerResult = results.First(r =>
            r.Request.Action == "Fireball" &&
            r.Request.SourceCharacterId == caster.Id);

        Assert.Contains(playerResult.Events, e => e.Event == "damage");
        Assert.True(playerResult.Events.Count(e => e.Event == "status_applied") >= 3);

        Assert.Contains(enemy1.StatusEffects, s => s.StatusType == StatusTypes.Burn);
        Assert.Contains(enemy2.StatusEffects, s => s.StatusType == StatusTypes.Burn);
        Assert.Contains(enemy3.StatusEffects, s => s.StatusType == StatusTypes.Burn);
    }

    [Fact]
    public async Task Fireball_WhenRedirected_BurnsOnlyBlocker()
    {
        var caster = new Warrior("caster");

        var blocker = new Skeleton
        {
            Id = "blocker",
            Name = "Blocker",
            Health = 30,
            MaxHealth = 30
        };

        var ally = new Skeleton
        {
            Id = "ally",
            Name = "Ally",
            Health = 30,
            MaxHealth = 30
        };

        blocker.StatusEffects.Add(new BlockStatus(blocker, ally));

        var encounter = new ActiveCombatEncounter(
            new List<Character> { caster },
            new List<Character> { blocker, ally });

        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = caster.Id,
            TargetCharacterId = ally.Id,
            Action = "Fireball"
        });

        Assert.NotEmpty(results);

        // Fireball 8 damage redirected to blocker, reduced by 50% => 4 + 2 burn damage = 6 total damage to the blocker
        Assert.Equal(30, ally.Health);
        Assert.Equal(24, blocker.Health);

        Assert.Contains(blocker.StatusEffects, s => s.StatusType == StatusTypes.Burn);
        Assert.DoesNotContain(ally.StatusEffects, s => s.StatusType == StatusTypes.Burn);
    }

    [Fact]
    public async Task FuryStrikes_ProducesMultipleDamageEvents()
    {
        var player = new Warrior("player");
        var enemy = new Skeleton { Id = "enemy1", Health = 20, MaxHealth = 20 };

        var encounter = new ActiveCombatEncounter(
            new List<Character> { player },
            new List<Character> { enemy });

        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = player.Id,
            TargetCharacterId = enemy.Id,
            Action = "Fury Strikes"
        });

        Assert.NotEmpty(results);

        var playerResult = results.First(r =>
            r.Request.Action == "Fury Strikes" &&
            r.Request.SourceCharacterId == player.Id);

        var damageEvents = playerResult.Events.Where(e => e.Event == "damage").ToList();

        Assert.InRange(damageEvents.Count, 2, 4);
        Assert.InRange(enemy.Health, 8, 14);
    }
}