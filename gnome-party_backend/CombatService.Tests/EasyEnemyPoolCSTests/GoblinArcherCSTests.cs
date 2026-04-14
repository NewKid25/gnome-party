using System;
using System.Collections.Generic;
using System.Text;
using GnomeParty.Database;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.EncounterData;
using Moq;
using Xunit;

namespace CombatService.Tests.EasyEnemyPoolCSTests
{
    public class GoblinArcherCSTests
    {
        /*******************************************************************************************************************/

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

        [Theory]
        [InlineData(0.64, 0.2, 0.8, 0.36, 0.0)]
        /* Test: Mage Ice Ray should reduce the damage of Goblin Archer's Piercing Arrow, but the Warrior's Block should not redirect it.

        // Piercing Arrow Chance: 0.64 (Needs to be 70% or less for success. Should succeed so long as target's health is above 50%)
        // Crippling Shot Chance: 0.2 (Needs to be 60% or less for success. Should fail since Piercing Arrow succeeded first)
        // Decision Breaker Roll: 0.8 (Shouldn't be used, but would pick Crippling Shot on >= 49%)
        // Priority targeting roll for each player class: 0.36 (Should choose Mage)
        //          * 0 - 49%: Mage
        //          * 50 - 79%: Warrior
        //          * 80 - 100% Bard 
        // Tie breaker between chosen class: 0.0 (Not used in this test)
        */
        public async Task PiercingArrowIgnoresBlockRedirectButNotReduction(
            double piercingArrowRoll,
            double cripplingShotRoll,
            double decisionBreakeerRoll,
            double targetingRoll,
            double targetTieRoll)
        {
            // Initialize random variables
            var rng = new TestRandomGenerator(piercingArrowRoll, cripplingShotRoll, decisionBreakeerRoll, targetingRoll, targetTieRoll);

            // Initialize characters for testing
            var mage = new Mage("mage") { Health = 20, MaxHealth = 20 };
            var warrior = new Warrior("warrior") { Health = 30, MaxHealth = 30 };
            var goblinArcher = new GoblinArcher() { Id = "goblinArcher", Health = 15, MaxHealth = 15 };

            // Create an encounter with the characters
            var encounter = new ActiveCombatEncounter(
                new List<Character> { mage, warrior },
                new List<Character> { goblinArcher }
            );

            // Create a mockdb and service for testing
            var mockDb = BuildDbMock(encounter);
            var service = new CombatService(mockDb.Object, rng);

            // Create combat requests for the Mage's Ice Ray and the Warrior's Block
            var mageResults = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = "game1",
                SourceCharacterId = mage.Id,
                TargetCharacterId = goblinArcher.Id,
                Action = "Ice Ray",
            });
            Assert.Empty(mageResults); // Should be empty since both players haven't readied up

            var warriorResults = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = "game1",
                SourceCharacterId = warrior.Id,
                TargetCharacterId = mage.Id,
                Action = "Block",
            });

            // Verify that combat results are not null and contain expected events
            Assert.NotNull(warriorResults);

            foreach (var result in warriorResults)
            {
                Console.WriteLine(
                    $"Source={result.Request.SourceCharacterId}, Action={result.Request.Action}, Target={result.Request.TargetCharacterId}");
            }
            Console.WriteLine($"Mage health: {mage.Health}");
            Console.WriteLine($"Warrior health: {warrior.Health}");
            Console.WriteLine($"Goblin Archer health: {goblinArcher.Health}");

            Assert.Equal(10, goblinArcher.Health); // Ice Ray should hit the Goblin Archer for 5 damage
            Assert.Equal(16, mage.Health); // Goblin Archer's Piercing Arrow should hit the Mage for 4 damage (reduced by 50% from 8 by Ice Ray and ignored Warrior's Block redirect) 
        }

        [Theory]
        [InlineData(0.88, 0.2, 0.33, 0.55, 0.0)]
        /* Test: Goblin Archer's Crippling Shot does approriate damage and reduces target's damage output

        // Piercing Arrow Chance: 0.88 (Needs to be 70% or less for success. Should fail)
        // Crippling Shot Chance: 0.2 (Needs to be 60% or less for success. Should succeed so long as target's health is 50% or less)
        // Decision Breaker Roll: 0.33 (Shouldn't be used, but would pick Piercing Shot on < 50%)
        // Priority targeting roll for each player class: 0.55 (Should choose Warrior)
        //          * 0 - 49%: Mage
        //          * 50 - 79%: Warrior
        //          * 80 - 100% Bard 
        // Tie breaker between chosen class: 0.0 (Not used in this test)
        */
        public async Task CripplingShotCorrectDamageAndWeakenStatus(
            double piercingArrowRoll,
            double cripplingShotRoll,
            double decisionBreakeerRoll,
            double targetingRoll,
            double targetTieRoll)
        {
            // Initialize random variables
            var rng = new TestRandomGenerator(
                piercingArrowRoll, 
                cripplingShotRoll, 
                decisionBreakeerRoll, 
                targetingRoll, 
                targetTieRoll, 
                piercingArrowRoll, 
                cripplingShotRoll, 
                decisionBreakeerRoll, 
                targetingRoll, 
                targetTieRoll);

            // Initialize characters for testing
            var warrior = new Warrior("warrior") { Health = 10, MaxHealth = 40 };
            var goblinArcher = new GoblinArcher() { Id = "goblinArcher", Health = 30, MaxHealth = 30 };

            // Create encounter, mockdb, and combat service for testing
            var encounter = new ActiveCombatEncounter(new List<Character> { warrior }, new List<Character> { goblinArcher });
            var mockDb = BuildDbMock(encounter);
            var service = new CombatService(mockDb.Object, rng);

            // -------------------------
            // ROUND 1
            // -------------------------

            // Execute combat request and verify results
            var round1Results = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = warrior.Id,
                TargetCharacterId = goblinArcher.Id,
                Action = "Slash",
            });

            Assert.NotEmpty(round1Results); // Verify we were passed round 1 results
            Assert.Equal(20, goblinArcher.Health); // Verify non debuffed Slash attack

            // -------------------------
            // TEST-ONLY ROUND RESET
            // -------------------------
            
            ResetEncounterForNextRound(encounter);

            // -------------------------
            // ROUND 2
            // -------------------------

            // Execute combat request and verify results
            var round2Results = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = warrior.Id,
                TargetCharacterId = goblinArcher.Id,
                Action = "Slash",
            });


            Assert.NotEmpty(round2Results); // Verify we were passed round 2 results
            Assert.Equal(13, goblinArcher.Health); // Verify debuffed Slash attack
        }

        private static void ResetEncounterForNextRound(ActiveCombatEncounter encounter)
        {
            if (encounter == null)
            {
                throw new ArgumentNullException(nameof(encounter));
            }

            for (int i = 0; i < encounter.PlayerReadied.Count; i++)
            {
                encounter.PlayerReadied[i] = false;
            }

            for (int i = 0; i < encounter.CombatRequests.Count; i++)
            {
                encounter.CombatRequests[i] = null;
            }
        }
    }
}