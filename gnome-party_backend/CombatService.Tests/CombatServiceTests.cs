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
    [Fact]
    public async Task TestCombatRequestHandlerAsync()
    {
        var mockDBService = new Mock<IDatabaseService>();
        var playerCharacter = new Character("test-source-character-id");
        var enemyCharacter = new Character("test-target-character-id");

        var encounter = new ActiveCombatEncounter([playerCharacter], [enemyCharacter]);
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
        var result = await combatService.CombatRequestHandlerAsync(request);
        Assert.NotNull(result);
        Assert.IsType<List<CombatResult>>(result);
    }
}