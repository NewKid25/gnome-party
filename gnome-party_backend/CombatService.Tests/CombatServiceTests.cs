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
    // Test: Verify that action is processed only after both players have submitted their actions
    public async Task SlashWithTwoPlayersProcessesAfterBothReadyAndProducesDamageEvent()
    {
        // Create two player characters and two enemy skeletons for the encounter
        var player1 = new Warrior("player1");
        var player2 = new Warrior("player2");
        var enemy1 = new Skeleton { Id = "enemy1", Health = 20, MaxHealth = 20 };
        var enemy2 = new Skeleton { Id = "enemy2", Health = 20, MaxHealth = 20 };

        // Create an encounter with the characters
        var encounter = new ActiveCombatEncounter(
            new List<Character> { player1, player2 },
            new List<Character> { enemy1, enemy2 });

        var mockDb = BuildDbMock(encounter); // Build the mock database to return our encounter when loaded
        var service = new CombatService(mockDb.Object); // Create the combat service with the mocked database

        // First player submits their action, but it should not be processed until both players have submitted an action
        var result1 = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = player1.Id,
            TargetCharacterId = enemy1.Id,
            Action = "Slash"
        });

        Assert.Empty(result1); // Check that no results were produced yet since both players have not submitted their actions

        // Second player submits their action, now both actions should be processed and damage events should be produced for both players' Slash actions
        var result2 = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = player2.Id,
            TargetCharacterId = enemy2.Id,
            Action = "Slash"
        });

        Assert.NotEmpty(result2); // Check that we got results back from the combat request handler after both players submitted their actions

        var playerSlashResults = result2 // Get the results for both players' Slash actions
            .Where(r => r.Request.Action == "Slash" &&
                        (r.Request.SourceCharacterId == player1.Id || r.Request.SourceCharacterId == player2.Id)).ToList();

        Assert.Equal(2, playerSlashResults.Count); // Check that we have results for both players' Slash actions

        var damageEvents = playerSlashResults // Extract the damage events from both players' Slash results
            .SelectMany(r => r.Events)
            .Where(e => e.Event == "damage")
            .ToList();

        Assert.Equal(2, damageEvents.Count); // Check that we have 2 damage events, one for each player's Slash action
        
        // Check that the correct damage was done to both enemies
        Assert.Equal(10, enemy1.Health);
        Assert.Equal(10, enemy2.Health);
    }

    [Fact]
    // Test: Verify that Bone Slash correctly applies damage to the target enemy 
    public async Task BoneSlashDealsCorrectDamage()
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
    public async Task MirrorCorrectlyDuplicatesFireball()
    {
        var mage = new Mage("mage") { Health = 78, MaxHealth = 78 };
    }
    [Fact]
    public async Task ParryPreventsEnemySlash()
    {
        var parryer = new Warrior("blocker") { Health = 30, MaxHealth = 30 }; // The character that will use Parry to block the enemy's attack
        var enemy = new Skeleton { Id = "enemy1", Health = 30, MaxHealth = 30 }; // The enemy that will attempt to attack the parryer.

        var encounter = new ActiveCombatEncounter( // Create an encounter with the parryer and the enemy
            new List<Character> { parryer },
            new List<Character> { enemy });

        var mockDb = BuildDbMock(encounter); // Build the mock database to return our encounter when loaded
        var service = new CombatService(mockDb.Object); // Create the combat service with the mocked database

        var results = await service.CombatRequestHandlerAsync(new CombatRequest // Make the combat request for the parryer to use Parry on the enemy's attack
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = parryer.Id,
            TargetCharacterId = enemy.Id,
            Action = "Parry"
        });

        Assert.NotEmpty(results); // Check that we got results back from the combat request handler
        Assert.Contains(parryer.StatusEffects, s => s is ParryStatus); // Check that the Parry status was applied to the enemy

        Assert.Equal(30, parryer.Health); // The parryer should take no damage from the enemy's attack because of the Parry status, so their health should remain at 30
        Assert.Equal(30, enemy.Health); // The enemy should also take no damage from the parry, so their health should remain at 30

        var enemyResult = results.FirstOrDefault(r => r.Request.SourceCharacterId == enemy.Id); // Find the result for the enemy's attack in the combat results
        Assert.NotNull(enemyResult); // Check that we found the result for the enemy's attack

        var damageEvent = enemyResult!.Events.FirstOrDefault(e => e.Event == "damage"); // Find the damage event in the enemy's attack result
        Assert.NotNull(damageEvent); // Check that we found the damage event in the enemy's attack result

        var damageParams = Assert.IsType<DamageEventParams>(damageEvent!.Params); // Validate that the damage event parameters are of the correct type
        Assert.Equal(parryer.Id, damageParams.TargetId); // The target of the enemy's attack should still be the parryer, even though the damage is prevented by the Parry status
        Assert.Equal(0, damageParams.DamageAmount); // The damage amount should be 0 because the Parry status should prevent all damage from the enemy's attack
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
    [Fact]
    public async Task IceRayAppliesChillStatusAndReducesEnemyAttackPower()
    {
        var mage = new Mage("mage") { Health = 20, MaxHealth = 20 }; // Create a mage character with 20 health
        var enemy = new Skeleton { Id = "enemy", Health = 30, MaxHealth = 30 }; // Create a skeleton enemy with 30 health

        var encounter = new ActiveCombatEncounter( // Create an encounter with the mage and the skeleton
            new List<Character> { mage },
            new List<Character> { enemy });

        var mockDb = BuildDbMock(encounter); // Build the mock database to return our encounter when loaded
        var service = new CombatService(mockDb.Object); // Create the combat service with the mocked database
        var results = await service.CombatRequestHandlerAsync(new CombatRequest // Make the combat request for the mage to use Ice Ray on the skeleton
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = mage.Id,
            TargetCharacterId = enemy.Id,
            Action = "Ice Ray"
        });

        Assert.NotEmpty(results); // Check that we got results back from the combat request handler
        Assert.Equal(25, enemy.Health); // The skeleton should have taken 5 damage from the Ice Ray
        Assert.Equal(17, mage.Health); // The mage should have taken 6 damage from the skeleton's attack, reduced by 50% because of the Chill status, for a total of 3 damage taken
    }
    [Fact]
    public async Task ChillStatusReducesOutgoingDamage()
    {
        var mage = new Mage("mage") { Health = 20, MaxHealth = 20 }; // Create a mage character with 20 health
        var enemy = new Skeleton { Id = "enemy", Health = 30, MaxHealth = 30 }; // Create a skeleton enemy with 30 health

        enemy.StatusEffects.Add(new ChillStatus(mage, enemy)); // Apply the Chill status to the skeleton

        var encounter = new ActiveCombatEncounter( // Create an encounter with the mage and the skeleton
            new List<Character> { mage },
            new List<Character> { enemy });

        var mockDb = BuildDbMock(encounter); // Build the mock database to return our encounter when loaded
        var service = new CombatService(mockDb.Object); // Create the combat service with the mocked database

        var results = await service.CombatRequestHandlerAsync(new CombatRequest // Make the combat request for the skeleton to use Slash on the mage
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = mage.Id,
            TargetCharacterId = enemy.Id,
            Action = "Magic Missile"
        });

        Assert.NotEmpty(results); // Check that we got results back from the combat request handler
        Assert.Equal(20, enemy.Health); // The skeleton should have taken 10 damage from the Magic Missile
        Assert.Equal(17, mage.Health); // Check that the mage took 3 damage from the skeleton's attack instead of 6 because of the Chill status

        var enemyResult = results.FirstOrDefault(r => r.Request.SourceCharacterId == enemy.Id && r.Request.Action == "Bone Slash"); // Find the result for the skeleton's attack in the combat results
        Assert.NotNull(enemyResult); // Check that we found the result for the skeleton's attack

        var damageEvent = enemyResult!.Events.FirstOrDefault(e => e.Event == "damage"); // Find the damage event in the skeleton's attack result
        Assert.NotNull(damageEvent); // Check that we found the damage event in the skeleton's attack result

        var damageParams = Assert.IsType<DamageEventParams>(damageEvent!.Params); // Validate that the damage event parameters are of the correct type
        Assert.Equal(mage.Id, damageParams.TargetId);
        Assert.Equal(3, damageParams.DamageAmount);
    }
}