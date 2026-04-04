using System.Numerics;
using GnomeParty.Database;
using Models;
using Models.Actions;
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
    public async Task SlashWithTwoPlayersProcessesAfterBothReadyAndProducesDamageEvent()
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
    public async Task BoneSlashWithOnePlayerDeals6Damage()
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
    public async Task BlockRedirectsEnemyAttackToBlockerAndReducesDamage()
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
    public async Task FireballNormalCastAppliesSplashBurn()
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
        Assert.True(playerResult.Events.Count(e => e.Event == "burn_status_applied") >= 3);

        Assert.Contains(enemy1.StatusEffects, s => s is BurnStatus);
        Assert.Contains(enemy2.StatusEffects, s => s is BurnStatus);
        Assert.Contains(enemy3.StatusEffects, s => s is BurnStatus);
    }
    [Fact]
    public async Task FireballWhenRedirectedBurnsOnlyBlocker()
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

        // Fireball 6 damage redirected to blocker, reduced by 50% => 3 + 2 burn damage = 5 total damage to the blocker
        Assert.Equal(30, ally.Health);
        Assert.Equal(25, blocker.Health);

        Assert.Contains(blocker.StatusEffects, s => s is BurnStatus);
        Assert.DoesNotContain(ally.StatusEffects, s => s is BurnStatus);
    }
    [Fact]
    public async Task FuryStrikesProducesMultipleDamageEvents()
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
    [Fact]
    public async Task MagicMissileIgnoresRattleGuardReduction()
    {
        var mage = new Mage("mage") { Health = 30, MaxHealth = 30 };
        var skeleton = new Skeleton { Id = "skeleton", Health = 20, MaxHealth = 20 };

        skeleton.StatusEffects.Add(new RattleGuardStatus(skeleton));

        var encounter = new ActiveCombatEncounter(
            new List<Character> { mage },
            new List<Character> { skeleton });

        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);
        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = mage.Id,
            TargetCharacterId = skeleton.Id,
            Action = "Magic Missile"
        });

        Assert.NotEmpty(results);
        Assert.Equal(10, skeleton.Health);

        var mageResult = results.FirstOrDefault(r =>
            r.Request.SourceCharacterId == mage.Id &&
            r.Request.Action == "Magic Missile");

        Assert.NotNull(mageResult);
        var damageEvent = mageResult!.Events.FirstOrDefault(e => e.Event == "damage");
        Assert.NotNull(damageEvent);
        var damageParams = Assert.IsType<DamageEventParams>(damageEvent!.Params);
        Assert.Equal(10, damageParams.DamageAmount);
    }
    [Fact]
    public async Task MagicMissileIgnoresParry()
    {
        var mage = new Mage
        {
            Id = "mage",
            Name = "Mage",
            Health = 30,
            MaxHealth = 30
        };
        var skelly = new Skeleton() { Id = "skelly", Name = "Skelly", Health = 30, MaxHealth = 30 };
        skelly.StatusEffects.Add(new ParryStatus(skelly, mage));
        var encounter = new ActiveCombatEncounter(
            new List<Character> { mage },
            new List<Character> { skelly });

        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = mage.Id,
            TargetCharacterId = skelly.Id,
            Action = "Magic Missile"
        });

        Assert.NotEmpty(results);

        Assert.Equal(24, mage.Health);
        Assert.Equal(20, skelly.Health);

        var mageResult = results.FirstOrDefault(r => r.Request.SourceCharacterId == mage.Id);
        Assert.NotNull(mageResult);
        Assert.Contains(mageResult!.Events, e => e.Event == "damage");
    }
    [Fact]
    public async Task MagicMissileRedirectedButFullDamage()
    {
        var caster = new Mage("caster");
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
            Action = "Magic Missile"
        });
        Assert.NotEmpty(results);
        // Block is attempted, so damage is redirected to blocker, but Magic Missile is unblockable so no damage reduction is applied
        Assert.Equal(30, ally.Health);
        Assert.Equal(20, blocker.Health);
    }
    [Fact]
    public async Task ParryPreventsEnemySlash()
    {
        var blocker = new Warrior("blocker") { Health = 30, MaxHealth = 30 };
        var enemy = new Skeleton { Id = "enemy1", Health = 30, MaxHealth = 30 };

        var encounter = new ActiveCombatEncounter(
            new List<Character> { blocker },
            new List<Character> { enemy });

        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        var firstResult = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = blocker.Id,
            TargetCharacterId = enemy.Id,
            Action = "Parry"
        });

        Assert.NotEmpty(firstResult);

        var secondResult = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = enemy.Id,
            TargetCharacterId = blocker.Id,
            Action = "Slash"
        });

        Assert.NotEmpty(secondResult);

        Assert.Equal(30, blocker.Health);
        Assert.Equal(30, enemy.Health);

        var enemyDamageResult = secondResult.FirstOrDefault(r => r.Request.SourceCharacterId == enemy.Id);
        Assert.NotNull(enemyDamageResult);
        Assert.Contains(enemyDamageResult!.Events, e => e.Event == "damage");
    }
    [Fact]
    public async Task WhirlingStrikeHitsEntireEnemyTeam()
    {
        var warrior = new Warrior("warrior") { Health = 42, MaxHealth = 42 }; // Should have 6 health remaining after taking 6 Skeleton Slashes

        // Six copies of a basic skeleton to test that Whirling Strike hits all enemies and that damage is calculated correctly for each
        var enemy1 = new Skeleton { Id = "enemy1" }; 
        var enemy2 = new Skeleton { Id = "enemy2" };
        var enemy3 = new Skeleton { Id = "enemy3" };
        var enemy4 = new Skeleton { Id = "enemy4" };
        var enemy5 = new Skeleton { Id = "enemy5" };
        var enemy6 = new Skeleton { Id = "enemy6" };

        var enemies = new List<Character> { enemy1, enemy2, enemy3, enemy4, enemy5, enemy6 }; // Save each enemy in a variable to check their health after the attack
        var encounter = new ActiveCombatEncounter(new List<Character> { warrior }, enemies); // Create encounter with the warrior and all six enemies

        var mockDb = BuildDbMock(encounter); // Build the mock database to return our encounter when loaded
        var service = new CombatService(mockDb.Object); // Create the combat service with the mocked database
        var results = await service.CombatRequestHandlerAsync(new CombatRequest // Make the combat request for the warrior to use Whirling Strike on one of the enemies. The target should be ignored and all enemies should be hit by the attack
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = warrior.Id,
            TargetCharacterId = enemy1.Id,
            Action = "Whirling Strike"
        });
        Assert.NotEmpty(results); // Check that we got results back from the combat request handler
        var playerResult = results.First(r => r.Request.Action == "Whirling Strike" && r.Request.SourceCharacterId == warrior.Id); // Find the result for the Whirling Strike action used by our warrior
        foreach (var enemy in enemies) // Loop through each enemy and check that they were hit by the attack and that the damage was calculated correctly. Each skeleton should take 5 damage from the Whirling Strike, so they should all have 15 health remaining
        {
            Assert.Equal(15, enemy.Health);
        }
        Assert.Equal(6, warrior.Health); // The warrior should have taken 6 damage from the skeletons' counterattacks, so they should have 6 health remaining
    }
    [Fact]
    public async Task RattleGuardReducesIncomingSlashDamageByHalf()
    {
        var player = new Warrior("player") { Health = 30, MaxHealth = 30 };
        var skeleton = new Skeleton { Id = "skeleton", Health = 20, MaxHealth = 20 };

        skeleton.StatusEffects.Add(new RattleGuardStatus(skeleton));

        var encounter = new ActiveCombatEncounter(
            new List<Character> { player },
            new List<Character> { skeleton });

        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);
        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = player.Id,
            TargetCharacterId = skeleton.Id,
            Action = "Slash"
        });

        Assert.NotEmpty(results);
        Assert.Equal(15, skeleton.Health);

        var playerResult = results.FirstOrDefault(r =>
            r.Request.SourceCharacterId == player.Id &&
            r.Request.Action == "Slash");

        Assert.NotNull(playerResult);

        var damageEvent = playerResult!.Events.FirstOrDefault(e => e.Event == "damage");
        Assert.NotNull(damageEvent);

        var damageParams = Assert.IsType<DamageEventParams>(damageEvent!.Params);
        Assert.Equal(skeleton.Id, damageParams.TargetId);
        Assert.Equal(5, damageParams.DamageAmount);
    }
}