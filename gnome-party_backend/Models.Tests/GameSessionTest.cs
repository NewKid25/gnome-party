using Models;
using Models.GameMetaData;
using Moq;


namespace Models.Tests
{
    public class GameSessionTest
    {
        [Fact]
        public void TestNewGameSession()
        {
            var connection = new GameConnection("test-connection-id", "test-player-id");

            var gameSession = new GameSession(connection);
            //temporary test to make sure the constructor is working as expected
            Assert.NotNull(gameSession);
        }
    }
}
