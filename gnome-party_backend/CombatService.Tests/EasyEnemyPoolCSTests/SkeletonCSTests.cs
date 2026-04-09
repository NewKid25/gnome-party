using GnomeParty.Database;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.EncounterData;
using Models.Status;
using Moq;
using Xunit;

namespace CombatService.Tests.EasyEnemyPoolCSTests;

public class SkeltonCSTests
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
    // Test: Bone Slash correctly applies damage to the target enemy 
    public async Task BoneSlashDealsCorrectDamage()
    {
        // Initialize player and enemy
        var player = new Warrior("player") { Health = 20, MaxHealth = 20 };
        var enemy = new Skeleton { Id = "enemy1", Health = 20, MaxHealth = 20 };

        // Create a combat encounter with the player and enemy
        var encounter = new ActiveCombatEncounter(
            new List<Character> { player },
            new List<Character> { enemy });

        // Initialize mock database and service to test combat service
        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        // Create a combat request and store the results
        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = player.Id,
            TargetCharacterId = enemy.Id,
            Action = "Slash"
        });

        Assert.NotEmpty(results); // Check that results were sent back

        var playerResult = results.First(r =>
            r.Request.Action == "Bone Slash" &&
            r.Request.SourceCharacterId == enemy.Id);

        var damageEvent = playerResult.Events.First(e => e.Event == "damage");
        var damageParams = Assert.IsType<DamageEventParams>(damageEvent.Params);

        // Test for appropriate damage event response and health of character after their attacks
        Assert.Equal(player.Id, damageParams.TargetId);
        Assert.Equal(6, damageParams.DamageAmount);
        Assert.Equal(10, enemy.Health);
        Assert.Equal(14, player.Health);
    }

    [Fact]
    // Test: Rattle Guard Reduced Slash Damage
    public async Task RattleGuardReducesIncomingSlashDamageByHalf()
    {
        // Initialize a player and a skeleton for testing
        var player = new Warrior("player") { Health = 30, MaxHealth = 30 };
        var skeleton = new Skeleton { Id = "skeleton", Health = 20, MaxHealth = 20 };

        // Manually add the Rattle Guard Status to the skeleton
        skeleton.StatusEffects.Add(new RattleGuardStatus(skeleton));

        // Create an encounter with the player and the skeleton
        var encounter = new ActiveCombatEncounter(
            new List<Character> { player },
            new List<Character> { skeleton });

        // Create the mock db and combat service for testing
        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        // Execute the combat request
        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = player.Id,
            TargetCharacterId = skeleton.Id,
            Action = "Slash"
        });

        Assert.NotEmpty(results); // Verify that a result was passed 
        Assert.Equal(15, skeleton.Health); // Verify that the incoming damage has been reduced

        // Retrieve the result from the combat request
        var playerResult = results.FirstOrDefault(r =>
            r.Request.SourceCharacterId == player.Id &&
            r.Request.Action == "Slash");

        Assert.NotNull(playerResult); // Verify that the result from the combat request is present

        // Verify the damage event and parameters
        var damageEvent = playerResult!.Events.FirstOrDefault(e => e.Event == "damage");
        Assert.NotNull(damageEvent);
        var damageParams = Assert.IsType<DamageEventParams>(damageEvent!.Params);
        Assert.Equal(skeleton.Id, damageParams.TargetId);
        Assert.Equal(5, damageParams.DamageAmount);
    }
}