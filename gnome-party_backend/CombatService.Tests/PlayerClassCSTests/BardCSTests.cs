using System.Diagnostics.Metrics;
using GnomeParty.Database;
using Models.Actions.BardActions;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.EncounterData;
using Models.Status;
using Moq;
using Xunit;
using static Models.CharacterData.PlayerCharacterClasses.Bard;

namespace CombatService.Tests.PlayerClassCSTests;

public class BardCSTests
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
    /*******************************************************************************************************************/

    [Fact]
    // Test: Discord does 8 damage and resets the current Bard song to Soothing Song
    public async Task DiscordAppliesDamageAndChangesBardSong()
    {
        // Initialize a bard and an enemy skeleton for testing
        var bard = new Bard("bard") { Health = 16, MaxHealth = 16 };
        var enemy = new Skeleton() { Id = "enemy",  Health = 30, MaxHealth = 30 };

        var encounter = new ActiveCombatEncounter( // Create the encounter with the bard and enemy skeleton
            new List<Character> { bard },
            new List<Character> { enemy }
        );

        var mockDb = BuildDbMock(encounter); // Build the mock database to return our encounter when loaded
        var service = new CombatService(mockDb.Object); // Create the combat service with the mocked database

        // Make the combat request for the bard to use Discord on the enemy skeleton
        var result = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = bard.Id,
            TargetCharacterId = enemy.Id,
            Action = "Discord"
        });

        // Verify the correct result was returned
        Assert.NotEmpty(result);

        var enemyResult = result.FirstOrDefault(res => res.Request.SourceCharacterId == enemy.Id);
        Assert.NotNull(enemyResult);

        Assert.Equal("Soothing Song", bard.CurrentSong);

        // Verfiy the correct damage has been done
        Assert.Equal(22, enemy.Health);
        Assert.Equal(10, bard.Health); // Keep a high help percentage to not trigger Rattle Guard option
    }

    [Fact]
    // Test: Mockery does 6 damage, applies Mock status to user, and making affected character target the user
    public async Task MockeryAttacksAndRedirectsEnemyAttention()
    {
        // The bard that will use mockery. Should take 6 damage from the enemy instead of the ally, leaving it with 10 health remaining
        var bard = new Bard("bard") { Health = 16, MaxHealth = 16 }; // Character that will use Mockery
        var ally = new Warrior() { Id = "ally", Health = 6, MaxHealth = 6 }; // Original target of enemy attack
        var enemy = new Skeleton() { Id = "enemy", Health = 26, MaxHealth = 26 }; // Enemy that will attack the ally warrior

        var encounter = new ActiveCombatEncounter( // Create the encounter with the bard, ally, and enemy
            new List<Character> { bard, ally },
            new List<Character> { enemy });

        var mockdb = BuildDbMock(encounter); // Build the mock database to return our encounter when loaded
        var service = new CombatService(mockdb.Object); // Create the combat service with the mocked database

        // Make the combat request for the bard to use Mockery on the enemy mage
        var result1 = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = bard.Id,
            TargetCharacterId = enemy.Id,
            Action = "Mockery"
        });

        // Make the combat request for the ally to use Slash on the enemy mage
        var result2 = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = ally.Id,
            TargetCharacterId = enemy.Id,
            Action = "Slash"
        });

        // Verify a result was passed
        Assert.NotEmpty(result2);
        var enemyResult = result2.FirstOrDefault(r => r.Request.SourceCharacterId == enemy.Id);
        Assert.NotNull(enemyResult);

        var enemyDamageEvent = enemyResult!.Events.FirstOrDefault(e => e.Event == "damage");
        Assert.NotNull(enemyDamageEvent);

        var damageParams = Assert.IsType<DamageEventParams>(enemyDamageEvent!.Params);
        Assert.Equal(6, damageParams.DamageAmount);
        Assert.Equal(bard.Id, damageParams.TargetId);

        Assert.Contains(bard.StatusEffects, s => s is MockStatus); // Verify Mock Status was applied to the bard

        // Verfiy that Mockery did 6 damage to the enemy mage, applied Mock Status to the bard, and redirected enemy's attack to the user
        Assert.Contains(bard.StatusEffects, s => s is MockStatus);
        Assert.Equal(10, enemy.Health); // Received 6 damage from the bard's mockery and 10 damage from the ally's slash
        Assert.Equal(10, bard.Health); // Received 6 damage from the enemy's Bone Slash instead of the ally being attacked
        Assert.Equal(6, ally.Health); // Received no damage froom the enemy mage
    }

    [Fact]
    // Test: Power Cord with Soothing Song heals all allies and the user
    public async Task PowerCordSoothingSongHealsAllAlliesAndUser()
    {
        // Initialize characters for testing
        var bard = new Bard("bard") { Health = 20, MaxHealth = 30 };
        var warrior1 = new Warrior("warrior1") { Health = 14, MaxHealth = 30 }; // Should gian +2 health (8 health from Power Cord - Soothing Song, -6 From Bone Slash) [for a total of 16]
        var warrior2 = new Warrior("warrior2") { Health = 20, MaxHealth = 30 }; // Should gain 8 health (for a total of 28)
        var warrior3 = new Warrior("warrior3") { Health = 20, MaxHealth = 30 }; // Should gain 8 health (for a total of 28)
        var enemy = new Skeleton() { Id = "skeleton", Health = 80, MaxHealth = 80 }; // Shoulf have 50 health after 3 instances of Warriors using Slash
        var allies = new List<Character> { bard, warrior1, warrior2, warrior3 };

        var encounter = new ActiveCombatEncounter(allies, new List<Character> { enemy }); // Create the encounter with the ally team and the enemy
        var mockdb = BuildDbMock(encounter); // Build the mock database to return our encounter when loaded
        var service = new CombatService(mockdb.Object); // Create the combat service with the mocked database

        bard.CurrentSong = BardSongs.Soothing; // Choose Soothing Song manually before executing Power Cord

        // Make the combat request for the bard to use Power Cord - Soothing Song on the ally team
        var result1 = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = bard.Id,
            TargetCharacterId = warrior2.Id,
            Action = "Power Cord"
        });

        Assert.Empty(result1); // Verify waiting until all player characters take an action

        // Make the combat requests for the Warriors to use Slash on the enemy
        var result2 = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = warrior1.Id,
            TargetCharacterId = enemy.Id,
            Action = "Slash"
        });

        Assert.Empty(result2); // Verify waiting until all player characters take an action

        var result3 = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = warrior2.Id,
            TargetCharacterId = enemy.Id,
            Action = "Slash"
        });

        Assert.Empty(result3); // Verify waiting until all player characters take an action

        var result4 = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = warrior3.Id,
            TargetCharacterId = enemy.Id,
            Action = "Slash"
        });

        Assert.NotEmpty(result4); // Verify all actions have been received

        // Check to see if the bard was properly stunned
        var bardResult = result4.FirstOrDefault(r => r.Request.SourceCharacterId == bard.Id);
        Assert.NotNull(bardResult);
        Assert.Contains(bardResult!.Events, e => e.Event == "stun_status_applied");
        Assert.DoesNotContain(bardResult.Events, e => e.Event == "damage");

        // Verify that healing attack damage happened properly
        Assert.Equal(28, bard.Health);
        Assert.Equal(16, warrior1.Health); // Healed +8 then slashed for -6 = +2 total
        Assert.Equal(28, warrior2.Health);
        Assert.Equal(28, warrior3.Health);
        Assert.Equal(50, enemy.Health); // 50 / 80 remaining after 3 Slash attacks
    }

    [Fact]
    // Test: Power Cord with Inspiring Song buffs all allies and the user
    public async Task PowerCordInspiringSongBuffsAllAlliesAndUser()
    {
        // Initialize characters for testing
        var bardPC = new Bard("bardPC") { Health = 20, MaxHealth = 20 };
        var bard1 = new Bard("bard1") { Health = 10, MaxHealth = 20 };
        var bard2 = new Bard("bard2") { Health = 20, MaxHealth = 20 };
        var bard3 = new Bard("bard3") { Health = 20, MaxHealth = 20 };
        var enemy = new Skeleton() { Id = "skeleton", Health = 72, MaxHealth = 72 }; // Should receive 36 damage ((8 * 1.5) * 3 ). Should have 36/72 left

        var allies = new List<Character> { bardPC, bard1, bard2, bard3 };
        
        var encounter = new ActiveCombatEncounter(allies, new List<Character> { enemy }); // Create the encounter with the ally team and the enemy
        var mockdb = BuildDbMock(encounter); // Build the mock database to return our encounter when loaded 
        var service = new CombatService(mockdb.Object); // Create the combat service with the mocked database

        bardPC.CurrentSong = BardSongs.Inspiring; // Choose Inspiring Song manually before executing Power Cord

        // Make the combat request for the bard to use Power Cord - Inspiring Song on the ally team
        var result1 = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = bardPC.Id,
            TargetCharacterId = bard1.Id,
            Action = "Power Cord"
        });

        Assert.Empty(result1); // Verify waiting until all player characters take an action

        // Make the combat request for the bard to use Discord on the enemy
        var result2 = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = bard1.Id,
            TargetCharacterId = enemy.Id,
            Action = "Discord"
        });

        Assert.Empty(result2); // Verify waiting until all player characters take an action

        // Make the combat request for the bard to use Discord on the enemy
        var result3 = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = bard2.Id,
            TargetCharacterId = enemy.Id,
            Action = "Discord"
        });

        Assert.Empty(result3); // Verify waiting until all player characters take an action       
        
        // Make the combat request for the bard to use Discord on the enemy
        var result4 = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = bard3.Id,
            TargetCharacterId = enemy.Id,
            Action = "Discord"
        });

        Assert.NotEmpty(result4); // Verify all actions have been received

        // Check to see if the bard was properly stunned
        var bardResult = result4.FirstOrDefault(r => r.Request.SourceCharacterId == bardPC.Id);
        Assert.NotNull(bardResult);
        Assert.Contains(bardResult!.Events, e => e.Event == "stun_status_applied");
        Assert.DoesNotContain(bardResult.Events, e => e.Event == "damage");

        // Verify boosted attack damage happened properly
        Assert.Equal(20, bardPC.Health);
        Assert.Equal(4, bard1.Health);
        Assert.Equal(20, bard2.Health);
        Assert.Equal(20, bard3.Health);
        Assert.Equal(36, enemy.Health);
    }

    [Fact]
    // Test: Power Cord with Frightening Song stuns all enemies and then Power Cord backlash stuns the user
    public async Task PowerCordFrighteningSongStunsAllEnemiesAndUser()
    {
        // Initialize the characters for testing
        var bardPC = new Bard("bardPC") { Health = 48, MaxHealth = 48 };
        var enemy1 = new Skeleton() { Id = "skeleton1", Health = 20, MaxHealth = 20 };
        var enemy2 = new Skeleton() { Id = "skeleton2", Health = 20, MaxHealth = 20 };
        var enemy3 = new Skeleton() { Id = "skeleton3", Health = 20, MaxHealth = 20 };
        var enemy4 = new Skeleton() { Id = "skeleton4", Health = 20, MaxHealth = 20 };

        var enemies = new List<Character>() {enemy1, enemy2, enemy3, enemy4};

        // Create the encounter, mockdb, and combat service for testing
        var encounter = new ActiveCombatEncounter(new List<Character> { bardPC}, enemies);
        var mockdb = BuildDbMock(encounter);
        var service = new CombatService(mockdb.Object);

        bardPC.CurrentSong = BardSongs.Frightening; // Manually change the Bard's song to Frightening Song

        // Exxecute Frightening Song
        var result = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId  = "game1",
            SourceCharacterId = bardPC.Id,
            TargetCharacterId = enemy2.Id,
            Action = "Power Cord",
        });

        Assert.NotEmpty(result); // Verify a combat result was passed

        // Verifty everyone was stunned
        var damageResults1 = result.FirstOrDefault(r => r.Request.SourceCharacterId == bardPC.Id);
        Assert.NotNull(damageResults1);
        Assert.Contains(damageResults1!.Events, e => e.Event == "stun_status_applied");
        Assert.DoesNotContain(damageResults1.Events, e => e.Event == "damage");
        
        // Verify that the stun prevented enemies from attacking
        var damageResults2 = result.FirstOrDefault(r => r.Request.SourceCharacterId == enemy2.Id);
        Assert.NotNull(damageResults2);
        Assert.Contains(damageResults2!.Events, e => e.Event == "stun_expired");
        Assert.DoesNotContain(damageResults2.Events, e => e.Event == "damage");

        var damageResults3 = result.FirstOrDefault(r => r.Request.SourceCharacterId == enemy4.Id);
        Assert.NotNull(damageResults3);
        Assert.Contains(damageResults3!.Events, e => e.Event == "stun_expired");
        Assert.DoesNotContain(damageResults3.Events, e => e.Event == "damage");

        // Verify the bard was not attacked (If the enemies weren't stunned they should've done 4 instances of Bone Slash [for 6 damage each])
        Assert.NotEqual(24, bardPC.Health);
        Assert.Equal(48, bardPC.Health);
    }

    [Fact]
    // Test: Song cycles through all songs
    public void SongCyclesThroughAllSongs()
    {
        // Initialize a bard, ally, and enemy for testing
        var bard = new Bard("bard");
        var ally = new Bard("ally");
        var enemy = new Skeleton { Id = "enemy" };

        // Test that the bard's current song cycles through all 3 songs properly
        var gameState = new CombatEncounterGameState(
            new List<Character> { bard, ally },
            new List<Character> { enemy });

        // Execute the song action 3 times to cycle through all songs
        var action = new Song(); 

        action.ResolveAttack(bard, ally, gameState);
        Assert.Equal(BardSongs.Inspiring, bard.CurrentSong);

        action.ResolveAttack(bard, ally, gameState);
        Assert.Equal(BardSongs.Frightening, bard.CurrentSong);

        action.ResolveAttack(bard, enemy, gameState);
        Assert.Equal(BardSongs.Soothing, bard.CurrentSong);
    }

    [Fact]
    // Test: Soothing Song heals the target and then cycles to Inspiring Song
    public async Task SongPlaysSoothingSongHealsAndThenCyclesToInspiringSong()
    {
        // Initialize bard, ally, and enemy for testing
        var bard = new Bard("bard")
        {
            Health = 25,
            MaxHealth = 25
        };
        var ally = new Warrior("ally")
        {
            Health = 10,
            MaxHealth = 30
        };
        var enemy = new Skeleton
        {
            Id = "enemy",
            Health = 20,
            MaxHealth = 20
        };

        bard.CurrentSong = BardSongs.Soothing; // Get the current bardic song

        // Create the encounter with the bard, ally, and enemy
        var encounter = new ActiveCombatEncounter(
            new List<Character> { bard, ally },
            new List<Character> { enemy });

        var mockDb = BuildDbMock(encounter); // Mock db for testing
        var service = new CombatService(mockDb.Object); // Make an instance of combat service for testing

        // Have the bard execute song
        var firstResult = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = bard.Id,
            TargetCharacterId = ally.Id,
            Action = "Song"
        });

        Assert.Empty(firstResult);

        // Have the ally use slash
        var secondResult = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = ally.Id,
            TargetCharacterId = enemy.Id,
            Action = "Slash"
        });

        Assert.NotEmpty(secondResult); // Make sure no results are sent until both player characters take an action
        Assert.Equal(12, ally.Health); // Healed for 8 health, but targeted by Skeleton for 6 Bone Slash damage
        Assert.Equal(BardSongs.Inspiring, bard.CurrentSong); // Verify that the bardic song has changed

        var bardResult = secondResult.FirstOrDefault(r =>
            r.Request.SourceCharacterId == bard.Id &&
            r.Request.Action == "Song");

        Assert.NotNull(bardResult);
        Assert.Contains(bardResult!.Events, e => e.Event == "healed");
    }

    [Fact]
    // Test: Inspiring Song buffs the ally and then cycles to Frightening Song
    public async Task SongPlaysInspiringSongAndBuffsAlliesNextAttack()
    {
        var bard = new Bard("bard")
        {
            Health = 25,
            MaxHealth = 25
        };

        var ally = new Warrior("ally")
        {
            Health = 30,
            MaxHealth = 30
        };

        var enemy = new Skeleton
        {
            Id = "enemy",
            Health = 20,
            MaxHealth = 20
        };

        bard.CurrentSong = BardSongs.Inspiring;

        var encounter = new ActiveCombatEncounter(
            new List<Character> { bard, ally },
            new List<Character> { enemy });

        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        var firstResult = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = bard.Id,
            TargetCharacterId = ally.Id,
            Action = "Song"
        });

        Assert.Empty(firstResult);

        var secondResult = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = ally.Id,
            TargetCharacterId = enemy.Id,
            Action = "Slash"
        });

        Assert.NotEmpty(secondResult);
        Assert.Equal(BardSongs.Frightening, bard.CurrentSong);

        // 20 HP enemy takes 15 from inspired Slash
        Assert.Equal(5, enemy.Health);

        var allyResult = secondResult.FirstOrDefault(r =>
            r.Request.SourceCharacterId == ally.Id &&
            r.Request.Action == "Slash");

        Assert.NotNull(allyResult);

        var damageEvent = allyResult!.Events.FirstOrDefault(e => e.Event == "damage");
        Assert.NotNull(damageEvent);

        var damageParams = Assert.IsType<DamageEventParams>(damageEvent!.Params);
        Assert.Equal(enemy.Id, damageParams.TargetId);
        Assert.Equal(15, damageParams.DamageAmount);
    }

    [Fact]
    // Test: Frightening Song stuns the target and then cycles to Soothing Song
    public async Task SongPlaysFrighteningSongAndStunsEnemy()
    {
        // Initialize bard, ally, and enemy for testing
        var bard = new Bard("bard")
        {
            Health = 25,
            MaxHealth = 25
        };

        var ally = new Warrior("ally")
        {
            Health = 30,
            MaxHealth = 30
        };

        var enemy = new Skeleton
        {
            Id = "enemy",
            Health = 20,
            MaxHealth = 20
        };

        bard.CurrentSong = BardSongs.Frightening; // Manually choose Frightening Song for testing

        // Initialize the combat encounter,  mock db, and the combat service
        var encounter = new ActiveCombatEncounter(
            new List<Character> { bard, ally },
            new List<Character> { enemy });
        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        // Execute Song - Frightening Song
        var firstResult = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = bard.Id,
            TargetCharacterId = enemy.Id,
            Action = "Song"
        });

        Assert.Empty(firstResult); // Verify a wait until the second player character has made an action request

        // Execute Slash
        var secondResult = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = ally.Id,
            TargetCharacterId = enemy.Id,
            Action = "Slash"
        });

        Assert.NotEmpty(secondResult); // Verify a result was passed
        Assert.Equal(BardSongs.Soothing, bard.CurrentSong); // Verify that the song has changed

        // Check to see that the enemy was stunned
        var enemyResult = secondResult.FirstOrDefault(r =>
            r.Request.SourceCharacterId == enemy.Id);
        Assert.NotNull(enemyResult);
        Assert.Contains(enemyResult!.Events, e => e.Event == "stunned");
        Assert.DoesNotContain(enemyResult.Events, e => e.Event == "damage");
    }

}