using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using GnomeParty.Database;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Models;
using Models.CharacterData;
using Models.CharacterData.BossEnemyPoolClasses;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.EncounterData;
using Models.GameMetaData;
using Models.Status;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace CombatService.Tests.BossEnemyPoolCSTests
{
    public class GnomeEaterCSTests
    {
        /*******************************************************************************************************************/
        //this test is just for debugging and should be replaced with more specific tests
        [Fact]
        public async Task TestCombatRequestHandlerAsync()
        {
            // Arrange
            var encounter = new ActiveCombatEncounter(
                new List<Character>
                {
                new Character("test-source-character-id"),
                new Character("test-source-character-id-2")
                },
                new List<Character>
                {
                new Skeleton { Id = "test-target-character-id" },
                new Skeleton()
                }
            );

            var mockDBService = new Mock<IDatabaseService>();
            mockDBService
                .Setup(dbService => dbService.LoadAsync<ActiveCombatEncounter>(It.IsAny<object>()))
                .ReturnsAsync(encounter);

            var combatService = new CombatService(mockDBService.Object, new TestRandomGenerator(0.0, 0.0));

            // Act
            var result1 = await combatService.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = "test-encounter-id",
                SourceCharacterId = "test-source-character-id",
                TargetCharacterId = "test-target-character-id",
                Action = "Slash",
            });

            var result2 = await combatService.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = "test-encounter-id",
                SourceCharacterId = "test-source-character-id-2",
                TargetCharacterId = "test-target-character-id",
                Action = "Slash",
            });

            // Assert
            Assert.NotNull(result1);
            Assert.IsType<List<CombatResult>>(result1);
        }

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

        private readonly ITestOutputHelper _output;

        public GnomeEaterCSTests(ITestOutputHelper output)
        {
            _output = output;
        }
        /*******************************************************************************************************************/

        [Theory]
        // Test: Crushing Strike correctly applies damage to the target enemy
   
        /* Test: Use Crushing Swipe on a Bard
        // Turn Count: 1
        // Player Class (enemy) with low health: 24
        // Player Class (enemies) Current Health: 34
        // Player Class (enemies) Max Health: 40
        // Gnome Eater Current Health: 138
        // Gnome Eater Max Health: 138
        // Devour Essence Chance Roll: 0.41 (Needs to be 60% or less for success. Should fail since health is above 50%)
        // Primal Roar Chance Roll: 0.5 (Needs to be 40% or less for success. Should fail)
        // Targeting Roll: 0.9 (Should return bard)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 100% Bard 
        // Tie breaking roll: 0.0      
        */
        [InlineData(1, 24, 34, 40, 138, 138, 0.41, 0.5, 0.9, 0.0, "Crushing Swipe", "target", "Bard", 74, 10)]

        /* Test: Crushing Strike correctly applies damage to the target enemy
        // Test: Use Crushing Swipe on a Bard
        // Turn Count: 7
        // Player Class (enemy) with low health: 24
        // Player Class (enemies) Current Health: 34
        // Player Class (enemies) Max Health: 40
        // Gnome Eater Current Health: 80
        // Gnome Eater Max Health: 80
        // Devour Essence Chance Roll: 0.85 (Needs to be 60% or less for success. Should fail)
        // Primal Roar Chance Roll: 0.84 (Needs to be 40% or less for success. Should fail)
        // Targeting Roll: 0.55 (Should return Mage)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 100% Bard 
        // Tie breaking roll: 0.0      
        */
        [InlineData(7, 24, 34, 40, 80, 80, 0.85, 0.84, 0.55, 0.0, "Crushing Swipe", "target", "Mage", 14, 10)]

        /* Test: Crushing Strike correctly applies damage to the target enemy
        // Test: Use Crushing Swipe on a Bard
        // Turn Count: 9
        // Player Class (enemy) with low health: 24
        // Player Class (enemies) Current Health: 34
        // Player Class (enemies) Max Health: 40
        // Gnome Eater Current Health: 192
        // Gnome Eater Max Health: 192
        // Devour Essence Chance Roll: 0.01 (Needs to be 60% or less for success. Should fail since health is above 50%)
        // Primal Roar Chance Roll: 0.74 (Needs to be 40% or less for success. Should fail)
        // Targeting Roll: 0.35 (Should return Mage)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 100% Bard 
        // Tie breaking roll: 0.0      
        */
        [InlineData(5, 24, 34, 40, 192, 192, 0.01, 0.74, 0.35, 0.0, "Crushing Swipe", "target", "Warrior", 126, 10)]

        // Test: Devour Essence correctly applies damage to the target enemy and heals the Gnome Eater

        /* Test: Use Devour Essence on a Bard
        // Turn Count: 9
        // Player Class (enemy) with low health: 24
        // Player Class (enemies) Current Health: 34
        // Player Class (enemies) Max Health: 40
        // Gnome Eater Current Health: 80
        // Gnome Eater Max Health: 80
        // Devour Essence Chance Roll: 0.01 (Needs to be 60% or less for success. Should succeed)
        // Primal Roar Chance Roll: 0.74 (Needs to be 40% or less for success. Should fail)
        // Targeting Roll: 0.88 (Should return Mage)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 100% Bard 
        // Tie breaking roll: 0.0      
        */
        [InlineData(9, 24, 34, 40, 80, 80, 0.01, 0.74, 0.88, 0.0, "Devour Essence", "target", "Bard", 24, 16)]

        /* Test: Use Devour Essence on a Mage
        // Turn Count: 2
        // Player Class (enemy) with low health: 24
        // Player Class (enemies) Current Health: 34
        // Player Class (enemies) Max Health: 40
        // Gnome Eater Current Health: 128
        // Gnome Eater Max Health: 128
        // Devour Essence Chance Roll: 0.01 (Needs to be 60% or less for success. Should succeed)
        // Primal Roar Chance Roll: 0.2 (Needs to be 40% or less for success. Should fail because Devour Essence triggered first)
        // Targeting Roll: 0.55 (Should return Mage)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 100% Bard 
        // Tie breaking roll: 0.0      
        */
        [InlineData(2, 24, 34, 40, 128, 128, 0.01, 0.2, 0.55, 0.0, "Devour Essence", "target", "Mage", 70, 16)]

        /* Test: Use Devour Essence on a Mage
        // Turn Count: 11
        // Player Class (enemy) with low health: 24
        // Player Class (enemies) Current Health: 34
        // Player Class (enemies) Max Health: 40
        // Gnome Eater Current Health: 69
        // Gnome Eater Max Health: 69
        // Devour Essence Chance Roll: 0.01 (Needs to be 60% or less for success. Should succeed)
        // Primal Roar Chance Roll: 0.2 (Needs to be 40% or less for success. Should fail because Devour Essence triggered first)
        // Targeting Roll: 0.55 (Should return Mage)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 100% Bard 
        // Tie breaking roll: 0.0      
        */
        [InlineData(11, 24, 34, 40, 69, 69, 0.01, 0.87, 0.21, 0.0, "Devour Essence", "target", "Warrior", 11, 16)]
        public async Task ValidateCrushingSwipeAndDevourEssence(
            int startingTurnCounut,
            int lowHealthEnemy,
            int currentEnemyHealth,
            int maxEnemyHealth,
            int gnomeEaterHealth,
            int gnomeEaterMaxHealth,
            double devRoll,
            double primRoarRoll,
            double targetingRoll,
            double targetTieBreaker,
            string expectedAction,
            string expectedTarget,
            string expectedClass,
            int expectedGnomeEaterHealthLeft,
            int expectedTargetHealthLeft)
        {
            var rng = new TestRandomGenerator(message => _output.WriteLine(message), devRoll, primRoarRoll, targetingRoll, targetTieBreaker);            
            
            // Create players and gnome eater for the test
            var gnomeEater = new GnomeEater() { Id = "gnomeEater", Health = gnomeEaterHealth, MaxHealth = gnomeEaterMaxHealth, turnCount = startingTurnCounut };
            var warrior1 = new Warrior("warrior1") { Health = currentEnemyHealth, MaxHealth = maxEnemyHealth }; // 10 damage with Slash
            var warrior2 = new Warrior("warrior2") { Health = currentEnemyHealth, MaxHealth = maxEnemyHealth }; // 10 damage with Slash
            var mage1 = new Mage("mage1") { Health = currentEnemyHealth, MaxHealth = maxEnemyHealth }; // 10 damage with Magic Missile
            var mage2 = new Mage("mage2") { Health = currentEnemyHealth, MaxHealth = maxEnemyHealth }; // 10 damage with Magic Missile
            var bard1 = new Bard("bard1") { Health = currentEnemyHealth, MaxHealth = maxEnemyHealth }; // 8 damage with Discord
            var bard2 = new Bard("bard2") { Health = currentEnemyHealth, MaxHealth = maxEnemyHealth }; // 8 damage with Discord

            // Create the test driven target
            Character target = new Character();
            if (expectedClass == "Bard")
            {
                target = new Bard(expectedTarget) { Health = lowHealthEnemy, MaxHealth = maxEnemyHealth }; // 8 damage with Discord
            }
            else if (expectedClass == "Warrior")
            {
                target = new Warrior(expectedTarget) { Health = lowHealthEnemy, MaxHealth = maxEnemyHealth }; // 10 damage with Slash
            }
            else
            {
                target = new Mage(expectedTarget) { Health = lowHealthEnemy, MaxHealth = maxEnemyHealth };// 10 damage with Magic Missile
            }

            var enemies = new List<Character> { warrior1, warrior2, mage1, mage2, bard1, bard2, target };
            var allies = new List<Character> { gnomeEater };

            // Create a combat encounter with the players and enemy
            var encounter = new ActiveCombatEncounter(enemies, allies);

            // Initialize mock database and service to test combat service
            var mockDb = BuildDbMock(encounter);
            var service = new CombatService(mockDb.Object, rng);

            /*
            _output.WriteLine("=== RNG Setup ===");
            _output.WriteLine($"devRoll = {devRoll}");
            _output.WriteLine($"primRoarRoll = {primRoarRoll}");
            _output.WriteLine($"targetingRoll = {targetingRoll}");
            _output.WriteLine("extra tie-break rolls = 0.0, 0.0");
            */

            // Create combat requests to simulate gnome eater being attacked
            var resultsW1 = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = warrior1.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Slash"
            });
            Assert.Empty(resultsW1); // Verify waiting until all player characters take an action

            var resultsW2 = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = warrior2.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Slash"
            });
            Assert.Empty(resultsW2); // Verify waiting until all player characters take an action

            var resultsM1 = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = mage1.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Magic Missile"
            });
            Assert.Empty(resultsM1); // Verify waiting until all player characters take an action

            var resultsM2 = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = mage2.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Magic Missile"
            });
            Assert.Empty(resultsM2); // Verify waiting until all player characters take an action

            var resultsB1 = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = bard1.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Discord"
            });
            Assert.Empty(resultsB1); // Verify waiting until all player characters take an action

            var resultsB2 = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = bard2.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Discord"
            });
            Assert.Empty(resultsB2); // Verify waiting until all player characters take an action

            var resultsT = new List<CombatResult>();
            if(target.CharacterType == "Warrior")
            {
                resultsT = await service.CombatRequestHandlerAsync(new CombatRequest
                {
                    EncounterId = encounter.EncounterId,
                    GameSessionId = "game1",
                    SourceCharacterId = target.Id,
                    TargetCharacterId = gnomeEater.Id,
                    Action = "Slash"
                });
            }
            else if(target.CharacterType == "Mage")
            {
                resultsT = await service.CombatRequestHandlerAsync(new CombatRequest
                {
                    EncounterId = encounter.EncounterId,
                    GameSessionId = "game1",
                    SourceCharacterId = target.Id,
                    TargetCharacterId = gnomeEater.Id,
                    Action = "Magic Missile"
                });
            }
            else
            {
                resultsT = await service.CombatRequestHandlerAsync(new CombatRequest
                {
                    EncounterId = encounter.EncounterId,
                    GameSessionId = "game1",
                    SourceCharacterId = target.Id,
                    TargetCharacterId = gnomeEater.Id,
                    Action = "Discord"
                });
            }
            Assert.NotEmpty(resultsT); // Check that results were sent back

            foreach (var result in resultsT)
            {
                _output.WriteLine(
                    $"Source={result.Request.SourceCharacterId}, Action={result.Request.Action}, Target={result.Request.TargetCharacterId}");
            }

            // Verify that the gnome eater took the correct action
            var gnomeEaterResult = resultsT.FirstOrDefault(r => r.Request.SourceCharacterId == gnomeEater.Id && r.Request.Action == expectedAction);
            Assert.NotNull(gnomeEaterResult);

            // verify that the weak health target was properly attacked
            // should take 14 damage (24/40 => 10/40)
            Assert.Equal(expectedTargetHealthLeft, target.Health);

            // verify that no other player character was attacked
            foreach (var enemy in enemies)
            {
                if (enemy.Id != target.Id)
                {
                    Assert.Equal(34, enemy.Health);
                }
            }

            /* Gnome Eater health should be:
            //      * If Bard Attacks: (Take 64 damage)
            //          * (2 * 10: 20 damage from Warriors, Slashes
            //          * (2 * 10: 20 damage from Mages, Magic Missiles
            //          * (2 * 8: 16 dammage from Bards, Discords
            //          * Extra 8 damage from Target Bard's Discord
            //      * If Warrior Attacks: (Take 66 damage)
            //          * (2 * 10: 20 damage from Warriors, Slashes
            //          * (2 * 10: 20 damage from Mages, Magic Missiles
            //          * (2 * 8: 16 dammage from Bards, Discords
            //          * Extra 10 damage from Target Warrior's Slash
            //      * If Mage Attacks: (Take 66 damage)
            //          * (2 * 10: 20 damage from Warriors, Slashes
            //          * (2 * 10: 20 damage from Mages, Magic Missiles
            //          * (2 * 8: 16 dammage from Bards, Discords
            //          * Extra 10 damage from Target Mage's Slash
            */
            Assert.Equal(expectedGnomeEaterHealthLeft, gnomeEater.Health);
        }


        [Fact]
        // Test: Primal Roar reduces the enemies attack power by roughly 25%
        public async Task PrimalRoarReducesEnemyTeamAttackPowerNextRound()
        {
            // Round 1 RNG:
            // devRoll = 0.99 -> Devour Essence fails
            // roarRoll = 0.39 -> Primal Roar succeeds
            // targetingRoll = 0.4 -> placeholder target selection
            // tieBreaker = 0.0
            //
            // Round 2 RNG:
            // values included just in case the boss acts again
            var rng = new TestRandomGenerator( 0.99, 0.39, 0.4, 0.0, 0.99, 0.99, 0.4, 0.0 );

            var gnomeEater = new GnomeEater
            {
                Id = "gnomeEater",
                Health = 80,
                MaxHealth = 80,
                turnCount = 2 // not 4th turn, so no Ravenous Growth
            };

            var warrior = new Warrior("warrior") { Health = 40, MaxHealth = 40 };
            var mage = new Mage("mage") { Health = 40, MaxHealth = 40 };
            var bard = new Bard("bard") { Health = 40, MaxHealth = 40 };

            var players = new List<Character> { warrior, mage, bard };
            var enemies = new List<Character> { gnomeEater };

            var encounter = new ActiveCombatEncounter(players, enemies);

            var mockDb = BuildDbMock(encounter);
            var service = new CombatService(mockDb.Object, rng);

            // -------------------------
            // ROUND 1
            // -------------------------

            var r1a = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = warrior.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Slash"
            });
            Assert.Empty(r1a);

            var r1b = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = mage.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Magic Missile"
            });
            Assert.Empty(r1b);

            var round1Results = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = bard.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Discord"
            });

            Assert.NotEmpty(round1Results);

            var primalRoarResult = round1Results.FirstOrDefault(r =>
                r.Request.SourceCharacterId == gnomeEater.Id &&
                r.Request.Action == "Primal Roar");

            Assert.NotNull(primalRoarResult);

            var gnomeEaterHealthAfterRound1 = gnomeEater.Health;

            // -------------------------
            // TEST-ONLY ROUND RESET
            // -------------------------
            ResetEncounterForNextRound(encounter);

            // -------------------------
            // ROUND 2
            // -------------------------
            // The whole point: player team should deal reduced damage this round.

            Assert.Contains(warrior.StatusEffects, s => s is FearStatus);
            Assert.Contains(mage.StatusEffects, s => s is FearStatus);
            Assert.Contains(bard.StatusEffects, s => s is FearStatus);

            var r2a = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = warrior.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Slash"
            });
            Assert.Empty(r2a);

            var r2b = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = mage.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Magic Missile"
            });
            Assert.Empty(r2b);

            var round2Results = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = bard.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Discord"
            });

            Assert.NotEmpty(round2Results);

            // Calculate how much damage players dealt in round 2 by comparing boss HP
            var damageDealtInRound2 = gnomeEaterHealthAfterRound1 - gnomeEater.Health;

            // Damage dealt: 
            // Warrior Slash => 10. Debuffed => 8
            // Mage Magic Missile = 10. Debuffed => 8
            // Bard Discord = 8. Defuffed => 6
            // Total = 28. Debuffed => 20
            Assert.Equal(20, damageDealtInRound2);
        }

        [Fact]
        // Test: Ravenous Growth permanently buffs attack power (use Crushing Swipe to test)
        public async Task RavenousGrowthPermanentlyStrengthensGnomeEater()
        {
            // Round 1 RNG: (Use Ravenous Growth)
            // devRoll = 0.32 -> Devour Essence succeeds (if not on the 4th turn)
            // roarRoll = 0.88 -> Primal Roar fails
            //
            // Round 2 RNG: (Buffed Crushing Swipe)
            // devRoll = 0.78 -> Devour Essence fails
            // roarRoll = 0.67 -> Primal Roar fails
            // targetingRoll = 0.32 -> target warrior
            // tieBreaker = 0.0
            //
            // Round 3 RNG: (Buffed Crushing Swipe)
            // devRoll = 0.99 -> Devour Essence fails
            // roarRoll = 0.59 -> Primal Roar fails
            // targetingRoll = 0.55 -> target mage
            // tieBreaker = 0.0
            var rng = new TestRandomGenerator(0.32, 0.88, 0.78, 0.67, 0.32, 0.0, 0.99, 0.59, 0.55, 0.0);

            var gnomeEater = new GnomeEater
            {
                Id = "gnomeEater",
                Health = 300,
                MaxHealth = 300,
                turnCount = 4 // Should use Ravenous Growth
            };

            var warrior = new Warrior("warrior") { Health = 17, MaxHealth = 40 };
            var mage = new Mage("mage") { Health = 17, MaxHealth = 40 };
            var bard = new Bard("bard") { Health = 40, MaxHealth = 40 };

            var players = new List<Character> { warrior, mage, bard };
            var enemies = new List<Character> { gnomeEater };

            var encounter = new ActiveCombatEncounter(players, enemies);

            var mockDb = BuildDbMock(encounter);
            var service = new CombatService(mockDb.Object, rng);

            // -------------------------
            // ROUND 1
            // -------------------------

            var r1a = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = warrior.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Slash"
            });
            Assert.Empty(r1a); // Verify waiting until all results are passed from players

            var r1b = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = mage.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Magic Missile"
            });
            Assert.Empty(r1b); // Verify waiting until all results are passed from players

            var round1Results = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = bard.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Discord"
            });

            Assert.NotEmpty(round1Results); // Verify results were passed now that all players have taken an action

            var ravenousGrowthResults = round1Results.FirstOrDefault(r =>
                r.Request.SourceCharacterId == gnomeEater.Id &&
                r.Request.Action == "Ravenous Growth");

            Assert.NotNull(ravenousGrowthResults);

            var gnomeEaterHealthAfterRound1 = gnomeEater.Health;

            _output.WriteLine($"TurnCount AFTER round1: {gnomeEater.turnCount}");

            foreach (var r in round1Results)
            {
                _output.WriteLine($"Source={r.Request.SourceCharacterId}, Action={r.Request.Action}");
            }

            // -------------------------
            // TEST-ONLY ROUND RESET
            // -------------------------
            ResetEncounterForNextRound(encounter);

            // -------------------------
            // ROUND 2
            // -------------------------

            var r2a = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = warrior.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Slash"
            });
            Assert.Empty(r2a); // Verify waiting until all results are passed from players

            var r2b = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = mage.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Magic Missile"
            });
            Assert.Empty(r2b); // Verify waiting until all results are passed from players

            var round2Results = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = bard.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Discord"
            });

            Assert.NotEmpty(round2Results); // Verify results were passed now that all players have taken an action

            var buffedCrushSwipe1 = round2Results.FirstOrDefault(r =>
            r.Request.SourceCharacterId == gnomeEater.Id &&
            r.Request.Action == "Crushing Swipe");
            Assert.NotNull(buffedCrushSwipe1);

            Assert.Equal(1, warrior.Health); // Check that the mage was targeted and hit with a buffed up Crushing Swipe

            // Calculate how much damage players dealt in round 2 by comparing boss HP
            var damageDealtInRound2 = gnomeEaterHealthAfterRound1 - gnomeEater.Health;

            // Damage dealt: 
            // Warrior Slash => 10.
            // Mage Magic Missile = 10.
            // Bard Discord = 8.
            // Total = 28.
            Assert.Equal(28, damageDealtInRound2);

            ResetEncounterForNextRound(encounter);

            // -------------------------
            // ROUND 3
            // -------------------------

            var r3a = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = warrior.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Slash"
            });
            Assert.Empty(r3a); // Verify waiting until all results are passed from players

            var r3b = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = mage.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Magic Missile"
            });
            Assert.Empty(r3b); // Verify waiting until all results are passed from players

            var round3Results = await service.CombatRequestHandlerAsync(new CombatRequest
            {
                EncounterId = encounter.EncounterId,
                GameSessionId = "game1",
                SourceCharacterId = bard.Id,
                TargetCharacterId = gnomeEater.Id,
                Action = "Discord"
            });

            Assert.NotEmpty(round3Results); // Verify results were passed now that all players have taken an action
            Assert.Equal(1, mage.Health); // Check that the mage was targeted and hit with a buffed up Crushing Swipe
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
