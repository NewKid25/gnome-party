using System;
using System.Collections.Generic;
using System.Text;
using CombatService.Tests;
using Models.AI.EasyEnemyPoolAI;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;

namespace Models.Tests.CharacterAITests.EasyEnemyPoolAITests
{
    public class ForestSpriteAITests
    {
        [Theory]
        /* Test: Use Entangle on a Bard
        // Entangle Chance Roll: 0.45 (Needs a <= 60% to succeed. Should succeed)
        // Targeting Roll: 0.33 (Should return Bard)
        //      * 0 - 49%: Bard
        //      * 50 - 79%: Mage
        //      * 80 - 100% Warrior  
        // Target Tie breaking roll: 0.0
        // 2nd Target Tie breaking roll: 0.0       
        */
        [InlineData(0.45, 0.33, 0.0, 0.0)]
        public async Task EntangleOnBardInsteadOfWarrior(
            double entangleRoll, 
            double targetingRoll, 
            double tieBreakerRoll, 
            double secondTieBreakerRoll)
        {
            var rng = new TestRandomGenerator(entangleRoll,  targetingRoll, tieBreakerRoll, secondTieBreakerRoll); // Simulate random numbers for the test
            var ai = new ForestSpriteAI(rng); // Create an instance of Forest Sprite AI

            // Create characters for testing
            var forestSprite = new ForestSprite() { Id = "forestSprite", Health = 20, MaxHealth = 20 };
            var warrior = new Warrior("warrior") { Health = 20, MaxHealth = 20 };
            var bard = new Bard("bard") { Health = 20, MaxHealth = 20 };
            var enemies = new List<Character> { bard, warrior };
            var allies = new List<Character> { forestSprite };

            // Manually insert the Forest Sprite actions
            var actions = new List<string> { "Leaf Dart" , "Entangle" };

            var playerRequests = new List<CombatRequest>
            {
                new CombatRequest
                {
                    SourceCharacterId = bard.Id,
                    Action = "Song"
                }
            };

            // Verify correct choice was made
            var request = ai.ChooseAction(forestSprite, actions, enemies, allies, playerRequests);
            Assert.Equal("Entangle", request.Action);
            var chosenTarget = enemies.First(e => e.Id == request.TargetCharacterId);
            Assert.Equal(bard.Id, chosenTarget.Id);
        }

        [Theory]
        /* Test: Use Leaf Dart on a Bard and Warrior
        // Entangle Chance Roll: 0.88 (Needs a <= 60% to succeed. Should fail)
        // Targeting Roll: 0.33 (Should return Bard)
        //      * 0 - 49%: Bard
        //      * 50 - 79%: Mage
        //      * 80 - 100% Warrior  
        // Target Tie breaking roll: 0.0
        // 2nd Target Tie breaking roll: 0.0       
        */
        [InlineData(0.88, 0.33, 0.0, 0.0)]
        public async Task LeafDartChoosesBothTargets(
            double entangleRoll,
            double targetingRoll,
            double tieBreakerRoll,
            double secondTieBreakerRoll)
        {
            var rng = new TestRandomGenerator(entangleRoll, targetingRoll, tieBreakerRoll, secondTieBreakerRoll); // Simulate random numbers for the test
            var ai = new ForestSpriteAI(rng); // Create an instance of Forest Sprite AI

            // Create characters for testing
            var forestSprite = new ForestSprite() { Id = "forestSprite", Health = 20, MaxHealth = 20 };
            var warrior = new Warrior("warrior") { Health = 20, MaxHealth = 20 };
            var bard = new Bard("bard") { Health = 20, MaxHealth = 20 };
            var enemies = new List<Character> { bard, warrior };
            var allies = new List<Character> { forestSprite };

            // Manually insert the Forest Sprite actions
            var actions = new List<string> { "Entangle", "Leaf Dart" };

            var playerRequests = new List<CombatRequest>
            {
                new CombatRequest
                {
                    SourceCharacterId = bard.Id,
                    Action = "Song"
                }
            };

            // Verify correct choice was made
            var request = ai.ChooseAction(forestSprite, actions, enemies, allies, playerRequests);
            Assert.Equal("Leaf Dart", request.Action);
        }
    }
}
