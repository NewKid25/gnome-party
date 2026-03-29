using Models;
using Models.CharacterData;

namespace Models.Tests
{
    public class CharacterTest
    {
        [Fact]
        public void TestNewCharacter()
        {
            var characterModel = new Character();
            //temporary test to make sure the constructor is working as expected
            Assert.Equal("Default Name", characterModel.Name);
            Assert.Equal(1, characterModel.Health);
        }
    }
}
