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
    public async Task SongPlaysSoothingSongAndThenCyclesToInspiringSong()
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

        bard.CurrentSong = BardSongs.Frightening;

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
            TargetCharacterId = enemy.Id,
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
        Assert.Equal(BardSongs.Soothing, bard.CurrentSong);

        var enemyResult = secondResult.FirstOrDefault(r =>
            r.Request.SourceCharacterId == enemy.Id);

        Assert.NotNull(enemyResult);

        // This assumes your stun logic prevents the attack and emits a stun event
        Assert.Contains(enemyResult!.Events, e => e.Event == "stunned");
        Assert.DoesNotContain(enemyResult.Events, e => e.Event == "damage");
    }

}