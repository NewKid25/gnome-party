using GnomeParty.Models;
using Moq;


namespace GnomeParty.Models.Tests
{
    public class GameSessionTest
    {
        [Fact]
        public void TestNewGameSession()
        {
            var connection = new Connection("test-connection-id", "test-player-id");

            var gameSession = new GameSession(connection);
            //temporary test to make sure the constructor is working as expected
            Assert.NotNull(gameSession);
        }
    }
}
