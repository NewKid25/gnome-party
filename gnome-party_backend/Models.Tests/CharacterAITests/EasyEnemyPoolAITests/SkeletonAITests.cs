using Models.AI;
using Models.AI.EasyEnemyPoolAI;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Xunit;

namespace Models.Tests.CharacterAITests.EasyEnemyPoolAI
{
    public class SkeletonAITests
    {
        [Theory] // Test cases for a skeleton with low health
        [InlineData(0.2, "Rattle Guard")]
        [InlineData(0.4, "Rattle Guard")]
        [InlineData(0.8, "Bone Slash")]
        // Test: Skeleton AI chooses "Rattle Guard" when health is low and random roll is less than or equal to 40%
        public void RattleGuardOnLowHealthAndCorrectRoll(double roll, string expectedAction)
        {
            var rng = new TestRandomGenerator(roll); // Create a TestRandomGenerator with the specified roll value
            var ai = new SkeletonAI(rng); // Create an instance of SkeletonAI using the TestRandomGenerator

            var skeleton = new Skeleton { Id = "skeleton", Health = 5, MaxHealth = 20 }; // Create a skeleton with low health (5 out of 20)
            var enemies = new List<Character> { new Warrior("warrior") { Health = 30, MaxHealth = 30 } }; // Create an enemy character (a warrior with full health)

            var actions = new List<string> { "Bone Slash", "Rattle Guard" }; // Define the available actions for the skeleton
            var request = ai.ChooseAction(skeleton, actions, enemies, new List<Character>()); // Get the action chosen by the AI

            Assert.Equal(expectedAction, request.Action); // Assert that the chosen action matches the expected action based on the roll value
        }
    }
}