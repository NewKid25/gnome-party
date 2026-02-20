using Models;

namespace Models.Tests
{
    public class CharacterModelTest
    {
        [Fact]
        public void TestNewCharacterModel()
        {
            var characterModel = new Character();
            //temporary test to make sure the constructor is working as expected
            Assert.Equal("Default Name", characterModel.name);
            Assert.Equal(1, characterModel.health);
        }
    }
}
