using Amazon.DynamoDBv2;
using Amazon.Lambda.TestUtilities;
using CombatService;
using GnomeParty.Database;
using Models;
using Models.CharacterData;
using Models.CombatData;
using Moq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace CombatService.Tests;

public class CombatServiceTests
{
    public CombatServiceTests()
    {

    }
    [Fact]
    public async Task TestCombatRequestHandlerAsync()
    {
        // Arrange
        var playerCharacter = new Character("test-source-character-id");
        var enemyCharacter = new Character("test-target-character-id");
        enemyCharacter.Health = 1;
        var encounter = new ActiveCombatEncounter([playerCharacter], [enemyCharacter]);
        var mockDBService = new Mock<IDatabaseService>();
        mockDBService.Setup(dbService => dbService.LoadAsync<ActiveCombatEncounter>(It.IsAny<object>()))
            .ReturnsAsync(encounter);
        var combatService = new CombatService(mockDBService.Object);
        var request = new CombatRequest
        {
            EncounterId = "test-encounter-id",
            SourceCharacterId = "test-source-character-id",
            TargetCharacterId = "test-target-character-id",
            Action = "Slash",
        };
        // Act
        var result = await combatService.CombatRequestHandlerAsync(request);
        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<CombatResult>>(result);
    }

    [Fact]
    public async Task TestProcessCombatRequestsAsync()
    {
        var mockDBService = new Mock<IDatabaseService>();
        var combatService = new CombatService(mockDBService.Object);


    }
}