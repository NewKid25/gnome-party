using System;
using System.Collections.Generic;
using System.Text;
using Models.AI.DifficultEnemyPoolAI;
using Models.CharacterData;
using Models.CharacterData.DifficultEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;

namespace Models.Tests.CharacterAITests.DifficultEnemyPoolAITests
{
    public class CaveBatAITests
    {
        [Theory]
        [InlineData(0.01, 6, 30, "Blood Peck")]
        [InlineData(0.5, 6, 32, "Blood Peck")]
        [InlineData(0.75, 9, 30, "Blood Peck")]
        // Test: Cave Bat AI chooses Blood Peck when a target has low health and a random roll is less than or equal to 75%
        public void BloodPeckOnLowHealthTargetAndCorrectRoll(double roll, int health, int maxHealth, string expectedAction)
        {
            var rng = new TestRandomGenerator(roll, 0.0); // Create a TestRandomGenerator with the specified roll value and tie breaker rng seed
            var ai = new CaveBatAI(rng); // Create an instance of CaveBatAI using the TestRandomGenerator

            // Create cave bats with some missing health
            var caveBat = new CaveBat { Id = "Cave Bat" };
            var caveBat1 = new CaveBat { Id = "Cave Bat1" };
            var caveBat2 = new CaveBat { Id = "Cave Bat2"};

            var allies = new List<Character> { caveBat1,  caveBat2 };

            // Create test enemy warriors
            var enemy1 = new Warrior("warrior1") { Health = health, MaxHealth = maxHealth };
            var enemy2 = new Warrior("warrior2") { Health = health, MaxHealth = maxHealth };
            var enemy3 = new Warrior("warrior3") { Health = health, MaxHealth = maxHealth };

            var enemies = new List<Character> { enemy1, enemy2, enemy3 };

            var actions = new List<string> { "Blood Peck", "Sonic Squeal" }; // Define the available actions for the cave bats

            var request1 = ai.ChooseAction(caveBat, actions, enemies, allies); // Get the action chosen by the AI
            Assert.Equal(expectedAction, request1.Action); // Assert that the chosen action matches the expected action based on the roll value
        }

        [Theory]
        [InlineData(0.90, 6, 30, "Sonic Squeal")]
        [InlineData(0.76, 9, 30, "Sonic Squeal")]
        [InlineData(0.1, 9, 30, "Blood Peck")]
        [InlineData(0.99, 9, 30, "Sonic Squeal")]
        public void SonicSquealLowHealthTargetButFailedRoll(double roll, int health, int maxHealth, string expectedAction)
        {
            var rng = new TestRandomGenerator(roll, 0.0); // Create a TestRandomGenerator with the specified roll value and tie breaker rng seed
            var ai = new CaveBatAI(rng); // Create an instance of CaveBatAI using the TestRandomGenerator

            // Create cave bats with some missing health
            var caveBat = new CaveBat { Id = "Cave Bat" };
            var caveBat1 = new CaveBat { Id = "Cave Bat1" };
            var caveBat2 = new CaveBat { Id = "Cave Bat2" };

            var allies = new List<Character> { caveBat1, caveBat2 };

            // Create test enemy warriors
            var enemy1 = new Warrior("warrior1") { Health = health, MaxHealth = maxHealth };
            var enemy2 = new Warrior("warrior2") { Health = health, MaxHealth = maxHealth };
            var enemy3 = new Warrior("warrior3") { Health = health, MaxHealth = maxHealth };

            var enemies = new List<Character> { enemy1, enemy2, enemy3 };

            var actions = new List<string> { "Blood Peck", "Sonic Squeal" }; // Define the available actions for the cave bats

            var request1 = ai.ChooseAction(caveBat, actions, enemies, allies); // Get the action chosen by the AI
            Assert.Equal(expectedAction, request1.Action); // Assert that the chosen action matches the expected action based on the roll value
        }
    }
}
