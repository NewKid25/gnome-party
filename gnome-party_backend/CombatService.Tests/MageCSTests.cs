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

public class MageCSTests
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
    // Test: Chill Status reduces the enemy's outgoing damage
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

    [Fact]
    // Test: Fireball hits the target, burns the target, and burns those adjacent to the target
    public async Task FireballNormalCastAppliesSplashBurn()
    {
        // Initialize the caster and sample enemies for testing
        var caster = new Mage("caster");
        var enemy1 = new Skeleton { Id = "enemy1", Health = 20, MaxHealth = 20 };
        var enemy2 = new Skeleton { Id = "enemy2", Health = 20, MaxHealth = 20 };
        var enemy3 = new Skeleton { Id = "enemy3", Health = 20, MaxHealth = 20 };

        // Create the encounter and initialize the service to test
        var encounter = new ActiveCombatEncounter(
            new List<Character> { caster },
            new List<Character> { enemy1, enemy2, enemy3 });

        // Initialize the mockdb and service
        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        // Cast fireball at enemy2, which should apply burn to enemy1, enemy2, and enemy3
        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = caster.Id,
            TargetCharacterId = enemy2.Id,
            Action = "Fireball"
        });

        // Check that results were returned
        Assert.NotEmpty(results);

        // Verify the correct results were passed 
        var playerResult = results.First(r =>
            r.Request.Action == "Fireball" &&
            r.Request.SourceCharacterId == caster.Id);

        Assert.Contains(playerResult.Events, e => e.Event == "damage");
        Assert.True(playerResult.Events.Count(e => e.Event == "burn_status_applied") >= 3);

        // Check that all the enemies were burned
        Assert.Contains(enemy1.StatusEffects, s => s is BurnStatus);
        Assert.Contains(enemy2.StatusEffects, s => s is BurnStatus);
        Assert.Contains(enemy3.StatusEffects, s => s is BurnStatus);
    }

    [Fact]
    // Test: Fireball, when redirected, only affects the blocker
    public async Task FireballWhenRedirectedBurnsOnlyBlocker()
    {
        // Initialize caster, blocker, and ally for testing
        var caster = new Mage("caster");
        var blocker = new Skeleton
        {
            Id = "blocker",
            Health = 30,
            MaxHealth = 30
        };
        var ally = new Skeleton
        {
            Id = "ally",
            Health = 30,
            MaxHealth = 30
        };

        // Manually give blocker the Block Status
        blocker.StatusEffects.Add(new BlockStatus(blocker, ally));

        // Create the encounter and service
        var encounter = new ActiveCombatEncounter(
            new List<Character> { caster },
            new List<Character> { blocker, ally });

        // Initialize the mockdb and service
        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        // Cast the fireball at ally
        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = caster.Id,
            TargetCharacterId = ally.Id,
            Action = "Fireball"
        });

        Assert.NotEmpty(results); // Ensure results isn't empty

        // Fireball 6 damage redirected to blocker, reduced by 50% => 3 + 2 burn damage = 5 total damage to the blocker
        Assert.Equal(30, ally.Health);
        Assert.Equal(25, blocker.Health);

        // Ensure that only the blocker receives burn damage
        Assert.Contains(blocker.StatusEffects, s => s is BurnStatus);
        Assert.DoesNotContain(ally.StatusEffects, s => s is BurnStatus);
    }

    [Fact]
    // Test: Ice Ray applies the correct status (Chill Status) and reduces the enemy's attack power
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
    // Test: Magic Missile ignores the damage reduction of Parry
    public async Task MagicMissileIgnoresParry()
    {
        // Initialize mage and skelly for testing 
        var mage = new Mage
        {
            Id = "mage",
            Name = "Mage",
            Health = 30,
            MaxHealth = 30
        };
        var skelly = new Skeleton() { Id = "skelly", Name = "Skelly", Health = 30, MaxHealth = 30 };

        // Manually add the Parry Status Effect to the skelly
        skelly.StatusEffects.Add(new ParryStatus(skelly, mage));

        // Initialize a combat encounter for testing
        var encounter = new ActiveCombatEncounter(
            new List<Character> { mage },
            new List<Character> { skelly });

        // Initialize a mockdb and combat service
        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        // Cast magic missile at the skelly
        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = mage.Id,
            TargetCharacterId = skelly.Id,
            Action = "Magic Missile"
        });

        Assert.NotEmpty(results); // Verify that results were passed back from Combat Request

        // Verify that the correct damage was done to the mage and skelly
        Assert.Equal(24, mage.Health);
        Assert.Equal(20, skelly.Health);

        var mageResult = results.FirstOrDefault(r => r.Request.SourceCharacterId == mage.Id);
        Assert.NotNull(mageResult);
        Assert.Contains(mageResult!.Events, e => e.Event == "damage");
    }

    [Fact]
    // Test: Magic Missile ignores the damage reduction of Rattle Guard
    public async Task MagicMissileIgnoresRattleGuardReduction()
    {
        // Initialize mage and skeleton for testing
        var mage = new Mage("mage") { Health = 30, MaxHealth = 30 };
        var skeleton = new Skeleton { Id = "skeleton", Health = 20, MaxHealth = 20 };

        // Manually add the Rattle Guard Status to the skeleton
        skeleton.StatusEffects.Add(new RattleGuardStatus(skeleton));

        // Create encounter and service
        var encounter = new ActiveCombatEncounter(
            new List<Character> { mage },
            new List<Character> { skeleton });

        // Initialize a mockdb and combat service
        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        // Create a combat request of mage doing Magic Missile to skeleton
        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = mage.Id,
            TargetCharacterId = skeleton.Id,
            Action = "Magic Missile"
        });

        Assert.NotEmpty(results); // Verify that a result was passed
        Assert.Equal(10, skeleton.Health); // Verify that Rattle Guard did not reduce Magic Missile's damage

        // Verify the correct results were passed
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
    // Test: Magic Missile is redirected but damage isn't reduced 
    public async Task MagicMissileRedirectedButFullDamage()
    {
        // Initialize caster, blocker, and ally for testing
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

        // Manually apply Block Status 
        blocker.StatusEffects.Add(new BlockStatus(blocker, ally));

        // Initialize encounter and service
        var encounter = new ActiveCombatEncounter(
            new List<Character> { caster },
            new List<Character> { blocker, ally });

        // Create a mockdb and service for testing
        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        // Create a combat request of caster calling Magic Missile
        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = caster.Id,
            TargetCharacterId = ally.Id,
            Action = "Magic Missile"
        });

        Assert.NotEmpty(results); // Verify that results were passed

        // Block is attempted, so damage is redirected to blocker, but Magic Missile is unblockable so no damage reduction is applied
        Assert.Equal(30, ally.Health);
        Assert.Equal(20, blocker.Health);
    }

    [Fact]
    // Test: Mirror duplicates Fireball and splits the damage
    public async Task MirrorCorrectlyDuplicatesFireball()
    {
        var mage = new Mage("mage") { Health = 78, MaxHealth = 78 }; // Should have 6 health remaining after taking 12 Bone Slashes from enemy Skeletons

        // Six copies of a basic skeleton to test that Fireball and Mirrored Fireball properly hit everyone
        var enemy1 = new Skeleton { Id = "enemy1" };
        var enemy2 = new Skeleton { Id = "enemy2" };
        var enemy3 = new Skeleton { Id = "enemy3" };
        var enemy4 = new Skeleton { Id = "enemy4" };
        var enemy5 = new Skeleton { Id = "enemy5" };
        var enemy6 = new Skeleton { Id = "enemy6" };

        var enemies = new List<Character> { enemy1, enemy2, enemy3, enemy4, enemy5, enemy6 };

        // Create the encounter and initialize the service to test
        var encounter = new ActiveCombatEncounter(new List<Character> { mage }, enemies);

        // Initialize the mockdb and service
        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        // Cast Mirror on enemy2
        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = mage.Id,
            TargetCharacterId = enemy2.Id,
            Action = "Mirror"
        });

        Assert.NotEmpty(results); // Check that results were returned 
        Assert.Contains(mage.StatusEffects, s => s is MirrorStatus); // Check that the Mirror Status was applied to the user
        Assert.Equal(42, mage.Health); // Check for the result of 6 enemy Bone Slash attacks

        // Cast Fireball on enemy5 which should also be mirrored onto enemy2
        var results2 = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = mage.Id,
            TargetCharacterId = enemy5.Id,
            Action = "Fireball",
        });

        Assert.NotEmpty(results2); // Check that results were returned from our attack

        // Verify the correct results were passed 
        var playerResult = results2.First(r =>
            r.Request.Action == "Fireball" &&
            r.Request.SourceCharacterId == mage.Id);
        Assert.Contains(playerResult.Events, e => e.Event == "damage");
        Assert.True(playerResult.Events.Count(e => e.Event == "burn_status_applied") >= 6);

        // Verify enemies 1, 3, 4, and 6 were only burned
        Assert.Equal(18, enemy1.Health);
        Assert.Equal(18, enemy3.Health);
        Assert.Equal(18, enemy4.Health);
        Assert.Equal(18, enemy6.Health);

        // Verify enemies 2 and 5 were hit with the fireball damage and burned
        Assert.Equal(12, enemy2.Health);
        Assert.Equal(12, enemy5.Health);

        Assert.Equal(6, mage.Health); // The mage should have taken 6 damage from the skeletons' counterattacks, so they should have 6 health remaining
    }

    [Fact]
    // Test: Mirror duplicates Ice Ray and applies 2 copies of Chill Status
    public async Task MirrorCorrectlyDuplicatesIceRay()
    {
        var mage = new Mage("mage") { Health = 24, MaxHealth = 24 }; // Should have 6 health remaining after taking 2 Full Damage Bone Slashes and 2 Reduced Bone Slashes

        // Create 2 skeleton enemies for testing
        // Both shoulf have 5 health left after taking an Ice Ray each
        var enemy1 = new Skeleton() { Id = "skeleton1", Health = 10, MaxHealth = 10 };
        var enemy2 = new Skeleton() { Id = "skeleton2", Health = 10, MaxHealth = 10 };

        var enemies = new List<Character> { enemy1, enemy2 };

        // Create the encounter and initialize the service to test
        var encounter = new ActiveCombatEncounter(new List<Character> { mage }, enemies);

        // Initialize the mockdb and service
        var mockDb = BuildDbMock(encounter);
        var service = new CombatService(mockDb.Object);

        // Cast Mirror on enemy1
        var results = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = mage.Id,
            TargetCharacterId = enemy1.Id,
            Action = "Mirror"
        });

        Assert.NotEmpty(results); // Check that results were returned 
        Assert.Contains(mage.StatusEffects, s => s is MirrorStatus); // Check that the Mirror Status was applied to the user
        Assert.Equal(12, mage.Health); // Check for the result of 2 Full Damage Bone Slash attacks

        // Cast Ice Ray on enemy2 which should also be mirrored onto enemy1
        var results2 = await service.CombatRequestHandlerAsync(new CombatRequest
        {
            EncounterId = encounter.EncounterId,
            GameSessionId = "game1",
            SourceCharacterId = mage.Id,
            TargetCharacterId = enemy2.Id,
            Action = "Ice Ray",
        });

        // Verify that both skeleton have taken 5 damage from Ice Ray
        Assert.Equal(5, enemy1.Health);
        Assert.Equal(5, enemy2.Health);

        Assert.Equal(6, mage.Health); // Check for the result of 2 reduced damage Bone Slashes
    }
}