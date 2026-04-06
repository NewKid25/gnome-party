using GnomeParty.Database;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.EncounterData;
using Models.Status;
using Moq;
using Xunit;

namespace CombatService.Tests.PlayerClassCSTests;
public class WarriorCSTests
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
    // Test: Block redirects attack 
    public async Task BlockRedirectsEnemyAttackToBlockerAndReducesDamage()
    {
        // Initialize an ally, blocker, and enemy for testing 
        var ally = new Warrior("ally") { Health = 30, MaxHealth = 30 };
        var blocker = new Warrior("blocker") { Health = 30, MaxHealth = 30 };
        var enemy = new Skeleton { Id = "enemy1", Health = 20, MaxHealth = 20 };

        // Create an encounter for testing
        var encounter = new ActiveCombatEncounter(
            new List<Character> { ally, blocker },
            new List<Character> { enemy });

        // Initialize a mockdb and combat service
        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        // Ally goes first and does slash
        var firstResult = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = ally.Id,
            TargetCharacterId = enemy.Id,
            Action = "Slash"
        });

        Assert.Empty(firstResult); // Make sure result is empty because blocker hasn't picked an action

        // Now the blocker blocks, which should redirect the attack and reduce its damage
        var secondResult = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = blocker.Id,
            TargetCharacterId = ally.Id,
            Action = "Block"
        });

        Assert.NotEmpty(secondResult); // Check that a result has been passed after blocker picked an action

        // Verify that ally took no damage and that blocker received the attack at a reduced amount
        Assert.Equal(30, ally.Health);
        Assert.Equal(27, blocker.Health);

        // Verify that the enemy attack was redirected and took place
        var enemyDamageResult = secondResult.FirstOrDefault(r => r.Request.SourceCharacterId == enemy.Id);
        Assert.NotNull(enemyDamageResult);
        Assert.Contains(enemyDamageResult!.Events, e => e.Event == "damage");
    }

    [Fact]
    // Test: Parry prevents enemy slash attack
    public async Task ParryPreventsEnemySlash()
    {
        var parryer = new Warrior("blocker") { Health = 30, MaxHealth = 30 }; // The character that will use Parry to block the enemy's attack
        var enemy = new Skeleton { Id = "enemy1", Health = 30, MaxHealth = 30 }; // The enemy that will attempt to attack the parryer.

        var encounter = new ActiveCombatEncounter( // Create an encounter with the parryer and the enemy
            new List<Character> { parryer },
            new List<Character> { enemy });

        var mockDb = BuildDbMock(encounter); // Build the mock database to return our encounter when loaded
        var service = new CombatService(mockDb.Object); // Create the combat service with the mocked database

        // Make the combat request for the parryer to use Parry on the enemy's attack
        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = parryer.Id,
            TargetCharacterId = enemy.Id,
            Action = "Parry"
        });

        Assert.NotEmpty(results); // Check that we got results back from the combat request handler
        Assert.Contains(parryer.StatusEffects, s => s is ParryStatus); // Check that the Parry status was applied to the user

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
    // Test: Parry prevents enemy Fireball damage, but not the burn damage to target and allies
    public async Task ParryPreventsFireballDamage()
    {
        // Initialize mage and warriors for testing
        var mage = new Mage("mage") { Health = 20, MaxHealth = 20 };
        var skeleton1 = new Skeleton() { Id = "skeleton1", Health = 30, MaxHealth = 30 };
        var skeleton2 = new Skeleton() { Id = "skeleton2", Health = 30, MaxHealth = 30 };
        var skeleton3 = new Skeleton() { Id = "skeleton3", Health = 30, MaxHealth = 30 };

        // Create an encounter with the mage and all three warriors
        var encounter = new ActiveCombatEncounter(
            new List<Character> { mage },
            new List<Character> { skeleton1, skeleton2, skeleton3 });

        var mockDb = BuildDbMock(encounter); // Build the mock database to return our encounter when loaded
        var service = new CombatService(mockDb.Object); // Create the combat service with the mocked database

        skeleton2.StatusEffects.Add(new ParryStatus(skeleton2, mage)); // Manually attach the parry status to skeleton2

        // Make the combat request for the mage to use Fireball on the skeletons
        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = mage.Id,
            TargetCharacterId = skeleton2.Id,
            Action = "Fireball"
        });

        Assert.NotEmpty(results); // Check that we got results back from the combat request handler

        // Verify the correct results were passed 
        var playerResult = results.First(r =>
            r.Request.Action == "Fireball" &&
            r.Request.SourceCharacterId == mage.Id);

        Assert.Contains(playerResult.Events, e => e.Event == "damage");
        Assert.True(playerResult.Events.Count(e => e.Event == "burn_status_applied") >= 3);

        // Check that all the enemies were burned
        Assert.Contains(skeleton1.StatusEffects, s => s is BurnStatus);
        Assert.Contains(skeleton2.StatusEffects, s => s is BurnStatus);
        Assert.Contains(skeleton3.StatusEffects, s => s is BurnStatus);

        // Check that all enemies took burn damage, but warrior parried the initial fireball damage
        Assert.Equal(28, skeleton1.Health);
        Assert.Equal(28, skeleton2.Health);
        Assert.Equal(28, skeleton3.Health);
    }

    [Fact]
    // Test: Action is processed only after both players have submitted their actions
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
    // Test: Whirling Strike hits the entire enemy team
    public async Task WhirlingStrikeHitsEntireEnemyTeam()
    {
        var warrior = new Warrior("warrior") { Health = 42, MaxHealth = 42 }; // Should have 6 health remaining after taking 6 Bone Slashes

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
        // Loop through each enemy and check that they were hit by the attack and that the damage was calculated correctly. Each skeleton should take 5 damage from the Whirling Strike, so they should all have 15 health remaining
        foreach (var enemy in enemies)
        {
            Assert.Equal(15, enemy.Health);
        }
        Assert.Equal(6, warrior.Health); // The warrior should have taken 6 damage from the skeletons' counterattacks, so they should have 6 health remaining
    }

}
