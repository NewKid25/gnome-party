using System;
using System.Collections.Generic;
using System.Text;
using CombatService.Tests;
using Models.AI.BossEnemyPoolAI;
using Models.CharacterData;
using Models.CharacterData.BossEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Xunit.Abstractions;

namespace Models.Tests.CharacterAITests.BossEnemyPoolAITests
{
    public class GnomeEaterAITests
    {

        [Theory]
        /* Test: Use Crushing Swipe on a Bard
        // Turn Count: 1
        // Player Class (enemy) with low health: 24
        // Player Class (enemies) Current Health: 34
        // Player Class (enemies) Max Health: 40
        // Gnome Eater Current Health: 30
        // Gnome Eater Max Health: 60
        // Devour Essence Chance Roll: 0.7 (Needs to be 60% or less for success. Should fail)
        // Primal Roar Chance Roll: 0.5 (Needs to be 40% or less for success. Should fail)
        // Targeting Roll: 0.9 (Should return bard)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 100% Bard  
        // Tie breaking roll: 0.0       */
        [InlineData(0, 24, 34, 40, 30, 60, 0.7, 0.5, 0.9, "Crushing Swipe", "target", "Bard", 0.0)]

        /* Test: Use Crushing Swipe on a Mage
        // Turn Count: 3
        // Player Class (enemy) with low health: 24
        // Player Class (enemies) Current Health: 34
        // Player Class (enemies) Max Health: 40
        // Gnome Eater Current Health: 30
        // Gnome Eater Max Health: 60
        // Devour Essence Chance Roll: 0.61 (Needs to be 60% or less for success. Should fail)
        // Primal Roar Chance Roll: 0.41 (Needs to be 40% or less for success. Should fail)
        // Targeting Roll: 0.77 (Should return Mage)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 100% Bard
        // Tie breaking roll: 0.0       */
        [InlineData(3, 24, 34, 40, 30, 60, 0.61, 0.41, 0.5, "Crushing Swipe", "target", "Mage", 0.0)]

        /* Test: Use Crushing Swipe on a Warrior
        // Turn Count: 7
        // Player Class (enemy) with low health: 6
        // Player Class (enemies) Current Health: 34
        // Player Class (enemies) Max Health: 40
        // Gnome Eater Current Health: 30
        // Gnome Eater Max Health: 60
        // Devour Essence Chance Roll: 0.99 (Needs to be 60% or less for success. Should fail)
        // Primal Roar Chance Roll: 0.99 (Needs to be 40% or less for success. Should fail)
        // Targeting Roll: 0.49 (Should return Warrior)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 100% Bard
        // Tie breaking roll: 0.0       */
        [InlineData(7, 6, 34, 40, 30, 60, .99, .99, 0.49, "Crushing Swipe", "target", "Warrior", 0.0)]
        public void ValidateCrushingStrike(
            int startingTurnCount,
            int lowHealthEnemy,
            int enemyHealth,
            int enemyMaxHealth,
            int gnomeEaterhealth,
            int gnomeEatermaxHealth,
            double devRoll,
            double primRoarRoll,
            double targetingRoll,
            string expectedAction,
            string expectedTargetId,
            string expectTargetClass,
            double targetingTieBreaker)
        {
            var rng = new TestRandomGenerator(devRoll, primRoarRoll, targetingRoll, targetingTieBreaker); // Simulate random numbers for the test
            var ai = new GnomeEaterAI(rng); // Create an instance of Gnome Eater AI

            // Create an instance of GnomeEater
            var gnomeEater = new GnomeEater { Id = "gnomeEater", Health = gnomeEaterhealth, MaxHealth = gnomeEatermaxHealth, turnCount = startingTurnCount };

            // Create player character to test the enemy ai against
            var warrior1 = new Warrior("warrior1") { Health = enemyHealth, MaxHealth = enemyMaxHealth };
            var warrior2 = new Warrior("warrior2") { Health = enemyHealth, MaxHealth = enemyMaxHealth };

            var mage1 = new Mage("mage1") { Health = enemyHealth, MaxHealth = enemyMaxHealth };
            var mage2 = new Mage("mage2") { Health = enemyHealth, MaxHealth = enemyMaxHealth };

            var bard1 = new Bard("bard1") { Health = enemyHealth, MaxHealth = enemyMaxHealth };
            var bard2 = new Bard("bard2") { Health = enemyHealth, MaxHealth = enemyMaxHealth };

            // Create the test driven target
            var target = new Character();
            if (expectTargetClass == "Bard")
            {
                target = new Bard(expectedTargetId) { Health = lowHealthEnemy, MaxHealth = enemyMaxHealth };
            }
            else if (expectTargetClass == "Warrior")
            {
                target = new Warrior(expectedTargetId) { Health = lowHealthEnemy, MaxHealth = enemyMaxHealth };
            }
            else
            {
                target = new Mage(expectedTargetId) { Health = lowHealthEnemy, MaxHealth = enemyMaxHealth };
            }

            var enemies = new List<Character> { warrior1, warrior2, mage1, mage2, bard1, bard2, target };
            var allies = new List<Character> { gnomeEater };

            var actions = new List<string> { "Crushing Swipe", "Devour Essence", "Primal Roar", "Ravenous Growth" };

            var request = ai.ChooseAction(gnomeEater, actions, enemies, allies);
            Assert.Equal(expectedAction, request.Action);
            var chosenTarget = enemies.First(e => e.Id == request.TargetCharacterId);
            Assert.Equal(expectedTargetId, chosenTarget.Id);
        }


        [Theory]
        /* Test: Use Devour Essence on a Warrior
        // Turn Count: 9
        // Player Class (enemy) with low health: 9
        // Player Class (enemies) Current Health: 34
        // Player Class (enemies) Max Health: 40
        // Gnome Eater Current Health: 30
        // Gnome Eater Max Health: 60
        // Devour Essence Chance Roll: 0.60 (Needs to be 60% or less for success. Should succeed)
        // Primal Roar Chance Roll: 0.88 (needs to be 40% or less for success. Should fail since Devour Essence succeeds)
        // Targeting Roll: 0.01 (Should return Warrior)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 100% Bard 
        // Target Roll Tiebreaker: 0.0
        */
        [InlineData(9, 9, 34, 40, 30, 60, 0.6, 0.88, 0.01, 0.0, "Devour Essence", "target", "Warrior")]

        /* Test: Use Devour Essence on a Mage
        // Turn Count: 6
        // Player Class (enemy) with low health: 15
        // Player Class (enemies) Current Health: 34
        // Player Class (enemies) Max Health: 40
        // Gnome Eater Current Health: 30
        // Gnome Eater Max Health: 60
        // Devour Essence Chance Roll: 0.15 (Needs to be 60% or less for success. Should succeed)
        // Primal Roar Chance Roll: 0.11 (needs to be 40% or less for success. Should fail since Devour Essence succeeds)
        // Targeting Roll: 0.77 (Should return Mage)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 100% Bard
        // Target Roll Tiebreaker: 0.0
        */
        [InlineData(6, 15, 34, 40, 30, 60, 0.15, 0.11, 0.77, 0.0, "Devour Essence", "target", "Mage")]

        /* Test: Use Devour Essence on a Bard
        // Turn Count: 11
        // Player Class (enemy) with low health: 15
        // Player Class (enemies) Current Health: 34
        // Player Class (enemies) Max Health: 40
        // Gnome Eater Current Health: 30
        // Gnome Eater Max Health: 60
        // Devour Essence Chance Roll: 0.22 (Needs to be 60% or less for success. Should succeed)
        // Primal Roar Chance Roll: 0.37 (needs to be 40% or less for success. Should fail since Devour Essence succeeds)
        // Targeting Roll: 0.8 (Should return Bard)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 100% Bard
        // Target Roll Tiebreaker: 0.0
        */
        [InlineData(11, 15, 34, 40, 30, 60, 0.22, 0.37, 0.8, 0.0, "Devour Essence", "target", "Bard")]
        public void ValidateDevourEssence(
            int startingTurnCount,
            int lowHealthEnemy,
            int enemyHealth,
            int enemyMaxHealth,
            int gnomeEaterhealth,
            int gnomeEatermaxHealth,
            double devRoll,
            double roarRoll,
            double targetingRoll,
            double targetTieBreaker,
            string expectedAction,
            string expectedTargetId,
            string expectTargetClass)
        {
            var rng = new TestRandomGenerator(devRoll, roarRoll, targetingRoll, targetTieBreaker);
            var ai = new GnomeEaterAI(rng); // Create an instance of Gnome Eater AI

            // Create an instance of GnomeEater
            var gnomeEater = new GnomeEater { Id = "gnomeEater", Health = gnomeEaterhealth, MaxHealth = gnomeEatermaxHealth, turnCount = startingTurnCount };

            // Create player character to test the enemy ai against
            var warrior1 = new Warrior("warrior1") { Health = enemyHealth, MaxHealth = enemyMaxHealth };
            var warrior2 = new Warrior("warrior2") { Health = enemyHealth, MaxHealth = enemyMaxHealth };

            var mage1 = new Mage("mage1") { Health = enemyHealth, MaxHealth = enemyMaxHealth };
            var mage2 = new Mage("mage2") { Health = enemyHealth, MaxHealth = enemyMaxHealth };

            var bard1 = new Bard("bard1") { Health = enemyHealth, MaxHealth = enemyMaxHealth };
            var bard2 = new Bard("bard2") { Health = enemyHealth, MaxHealth = enemyMaxHealth };

            // Create the test driven target
            var target = new Character();
            if (expectTargetClass == "Bard")
            {
                target = new Bard(expectedTargetId) { Health = lowHealthEnemy, MaxHealth = enemyMaxHealth };
            }
            else if (expectTargetClass == "Warrior")
            {
                target = new Warrior(expectedTargetId) { Health = lowHealthEnemy, MaxHealth = enemyMaxHealth };
            }
            else
            {
                target = new Mage(expectedTargetId) { Health = lowHealthEnemy, MaxHealth = enemyMaxHealth };
            }

            var enemies = new List<Character> { warrior1, warrior2, mage1, mage2, bard1, bard2, target };

            var allies = new List<Character> { gnomeEater };
            var actions = new List<string> { "Crushing Swipe", "Devour Essence", "Primal Roar", "Ravenous Growth" };

            var request = ai.ChooseAction(gnomeEater, actions, enemies, allies);
            var chosenTarget = enemies.First(e => e.Id == request.TargetCharacterId);

            Assert.Equal(expectedAction, request.Action);
            Assert.Equal(expectedTargetId, chosenTarget.Id);
        }

        [Theory]
        /* Test: Use Primal Roar (fake target is Warrior)
        // Turn Count: 2
        // Player Class (enemy) with low health: 6
        // Player Class (enemies) Current Health: 34
        // Player Class (enemies) Max Health: 40
        // Gnome Eater Current Health: 30
        // Gnome Eater Max Health: 60
        // Devour Essence Chance Roll: 0.99 (Needs to be 60% or less for success. Should fail)
        // Primal Roar Chance Roll: 0.39 (Needs to be 40% or less for success. Should succeed)
        // Targeting Roll: 0.4 (Should return Warrior. But Primal Roar should still hit all enemies)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 99% Bard 
        // Target Tie Breaker: 0.0
        */
        [InlineData(2, 6, 34, 40, 30, 60, 0.99, 0.39, 0.4, "Primal Roar", "target", "Warrior", 0.0)]

        /* Test: Use Primal Roar (fake target is Mage)
        // Turn Count: 5
        // Player Class (enemy) with low health: 6
        // Player Class (enemies) Current Health: 34
        // Player Class (enemies) Max Health: 40
        // Gnome Eater Current Health: 30
        // Gnome Eater Max Health: 60
        // Devour Essence Chance Roll: 0.99 (Needs to be 60% or less for success. Should fail)
        // Primal Roar Chance Roll: 0.4 (Needs to be 40% or less for success. Should succeed)
        // Targeting Roll: 0.5 (Should return Mage. But Primal Roar should still hit all enemies)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 99% Bard 
        // Target Tie Breaker: 0.0
        */
        [InlineData(5, 6, 34, 40, 30, 60, 0.99, 0.4, 0.5, "Primal Roar", "target", "Mage", 0.0)]

        /* Test: Use Primal Roar (fake target is Bard)
        // Turn Count: 5
        // Player Class (enemy) with low health: 6
        // Player Class (enemies) Current Health: 34
        // Player Class (enemies) Max Health: 40
        // Gnome Eater Current Health: 30
        // Gnome Eater Max Health: 60
        // Devour Essence Chance Roll: 0.99 (Needs to be 60% or less for success. Should fail)
        // Primal Roar Chance Roll: 0.21 (Needs to be 40% or less for success. Should succeed)
        // Targeting Roll: 0.99 (Should return Mage. But Primal Roar should still hit all enemies)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 99% Bard 
        // Target Tie Breaker: 0.0
        */
        [InlineData(5, 6, 34, 40, 30, 60, 0.99, 0.21, 0.99, "Primal Roar", "target", "Bard", 0.0)]
        public void ValidatePrimalRoar(
            int startingTurnCount,
            int lowHealthEnemy,
            int enemyHealth,
            int enemyMaxHealth,
            int gnomeEaterhealth,
            int gnomeEatermaxHealth,
            double devRoll,
            double primRoarRoll,
            double targetingRoll,
            string expectedAction,
            string expectedTargetId,
            string expectTargetClass,
            double tieBreakingRoll)
        {
            var rng = new TestRandomGenerator(devRoll, primRoarRoll, targetingRoll, tieBreakingRoll);
            var ai = new GnomeEaterAI(rng); // Create an instance of Gnome Eater AI

            // Create an instance of GnomeEater
            var gnomeEater = new GnomeEater { Id = "gnomeEater", Health = gnomeEaterhealth, MaxHealth = gnomeEatermaxHealth, turnCount = startingTurnCount };

            // Create player character to test the enemy ai against
            var warrior1 = new Warrior("warrior1") { Health = enemyHealth, MaxHealth = enemyMaxHealth };
            var warrior2 = new Warrior("warrior2") { Health = enemyHealth, MaxHealth = enemyMaxHealth };

            var mage1 = new Mage("mage1") { Health = enemyHealth, MaxHealth = enemyMaxHealth };
            var mage2 = new Mage("mage2") { Health = enemyHealth, MaxHealth = enemyMaxHealth };

            var bard1 = new Bard("bard1") { Health = enemyHealth, MaxHealth = enemyMaxHealth };
            var bard2 = new Bard("bard2") { Health = enemyHealth, MaxHealth = enemyMaxHealth };

            // Create the test driven target
            var target = new Character();
            if (expectTargetClass == "Bard")
            {
                target = new Bard(expectedTargetId) { Health = lowHealthEnemy, MaxHealth = enemyMaxHealth };
            }
            else if (expectTargetClass == "Warrior")
            {
                target = new Warrior(expectedTargetId) { Health = lowHealthEnemy, MaxHealth = enemyMaxHealth };
            }
            else
            {
                target = new Mage(expectedTargetId) { Health = lowHealthEnemy, MaxHealth = enemyMaxHealth };
            }

            var enemies = new List<Character> { warrior1, warrior2, mage1, mage2, bard1, bard2, target };
            var allies = new List<Character> { gnomeEater };

            var actions = new List<string> { "Crushing Swipe", "Devour Essence", "Primal Roar", "Ravenous Growth" };
            var request = ai.ChooseAction(gnomeEater, actions, enemies, allies);
            var chosenTarget = enemies.First(e => e.Id == request.TargetCharacterId);

            Assert.Equal(expectedAction, request.Action);
            Assert.Equal(expectedTargetId, chosenTarget.Id);

        }

        [Theory]
        // Test: Use Ravenous Growth on turn 8
        [InlineData (8, "Ravenous Growth", "gnomeEater")]
        // Test: Use Ravenous Growth on turn 4
        [InlineData(4, "Ravenous Growth", "gnomeEater")]
        // Test: Use Ravenous Growth on turn 12
        [InlineData(12, "Ravenous Growth", "gnomeEater")]
        public void ValidateRavenousGrowth(
            int startingTurnCount, 
            string expectedAction, 
            string expectedTargetId)
        {
            var ai = new GnomeEaterAI();
            var gnomeEater = new GnomeEater() { Id = "gnomeEater", turnCount = startingTurnCount };

            var bard = new Bard("bard");
            var mage = new Mage("mage");
            var warrior = new Warrior("warrior");

            var enemies = new List<Character> { bard, mage, warrior };
            var allies = new List<Character> { gnomeEater };

            var actions = new List<string> { "Crushing Swipe", "Devour Essence", "Primal Roar", "Ravenous Growth" };
            var request = ai.ChooseAction(gnomeEater, actions, enemies, allies);

            Assert.Equal(expectedAction, request.Action);
            Assert.Equal(expectedTargetId, gnomeEater.Id);
        }

    }
}
