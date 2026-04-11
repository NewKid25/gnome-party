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
        //      * 80 - 100% Bard  */
        [InlineData(0, 24, 34, 40, 30, 60, 0.7, 0.5, 0.9, "Crushing Swipe", "target", "Bard")]

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
        //      * 80 - 100% Bard   */
        [InlineData(3, 24, 34, 40, 30, 60, 0.61, 0.41, 0.5, "Crushing Swipe", "target", "Mage")]

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
        //      * 80 - 100% Bard  */
        [InlineData(7, 6, 34, 40, 30, 60, .99, .99, 0.49, "Crushing Swipe", "target", "Warrior")]
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
            string expectTargetClass)
        { 
            var rng = new TestRandomGenerator(devRoll, primRoarRoll, targetingRoll); // Simulate random numbers for the test
            var ai = new GnomeEaterAI(rng); // Create an instance of Gnome Eater AI

            // Create an instance of GnomeEater
            var gnomeEater = new GnomeEater { Id = "gnomeEater", Health = gnomeEaterhealth, MaxHealth = gnomeEatermaxHealth, turnCounter = startingTurnCount };

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

            var enemies = new List<Character> { warrior1, warrior2, mage1, mage2,  bard1, bard2, target };
            var allies = new List<Character> { gnomeEater };

            var actions = new List<string> { "Crushing Swipe","Devour Essence", "Primal Roar", "Ravenous Growth" };

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
        // Targeting Roll: 0.0 (Should return Warrior)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 100% Bard  */
        [InlineData(9, 9, 34, 40, 30, 60, 0.6, 0.01, "Devour Essence", "target", "Warrior")]

        /* Test: Use Devour Essence on a Mage
        // Turn Count: 11
        // Player Class (enemy) with low health: 15
        // Player Class (enemies) Current Health: 34
        // Player Class (enemies) Max Health: 40
        // Gnome Eater Current Health: 30
        // Gnome Eater Max Health: 60
        // Devour Essence Chance Roll: 0.22 (Needs to be 60% or less for success. Should succeed)
        // Targeting Roll: 0.0 (Should return Warrior)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 100% Bard */
        [InlineData(11, 15, 34, 40, 30, 60, 0.22, 0.5, "Devour Essence", "target", "Mage")]

        /* Test: Use Devour Essence on a Bard
        // Turn Count: 11
        // Player Class (enemy) with low health: 15
        // Player Class (enemies) Current Health: 34
        // Player Class (enemies) Max Health: 40
        // Gnome Eater Current Health: 30
        // Gnome Eater Max Health: 60
        // Devour Essence Chance Roll: 0.22 (Needs to be 60% or less for success. Should succeed)
        // Targeting Roll: 0.0 (Should return Warrior)
        //      * 0 - 49%: Warrior
        //      * 50 - 79%: Mage
        //      * 80 - 100% Bard */
        [InlineData(11, 15, 34, 40, 30, 60, 0.22, 0.8, "Devour Essence", "target", "Bard")]
        public void ValidateDevourEssence(
            int startingTurnCount,
            int lowHealthEnemy,
            int enemyHealth,
            int enemyMaxHealth,
            int gnomeEaterhealth,
            int gnomeEatermaxHealth,
            double devRoll,
            double targetingRoll,
            string expectedAction,
            string expectedTargetId,
            string expectTargetClass)
        {
            var rng = new TestRandomGenerator(devRoll, targetingRoll); // Simulate random numbers for the test
            var ai = new GnomeEaterAI(rng); // Create an instance of Gnome Eater AI

            // Create an instance of GnomeEater
            var gnomeEater = new GnomeEater { Id = "gnomeEater", Health = gnomeEaterhealth, MaxHealth = gnomeEatermaxHealth, turnCounter = startingTurnCount };

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


    }
}
