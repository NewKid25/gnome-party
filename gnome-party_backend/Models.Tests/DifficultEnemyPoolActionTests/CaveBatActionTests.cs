using System;
using System.Collections.Generic;
using System.Text;
using Models.Actions.DifficultEnemyPoolActions.CaveBatActions;
using Models.CharacterData;
using Models.CharacterData.DifficultEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;

namespace Models.Tests.DifficultEnemyPoolActionTests
{
    public class CaveBatActionTests
    {
        [Fact]
        // Test: Sonic Squeal deals 3 damage to each enemy
        public void SonicSquealDealDamageToAllEnemies()
        {
            // Initialize user and targets for testing
            var user = new CaveBat() { Id = "caveBat" };
            var target1 = new Mage("player1");
            var target2 = new Mage("player2");
            var target3 = new Mage("player3");
            var target4 = new Mage("player4");

            var mageTeam = new List<Character> { target1, target2, target3, target4 };

            // Create a combat encounter with user and mage team
            var gameState = new CombatEncounterGameState( mageTeam, new List<Character> { user });

            // Execute attack action
            var action = new SonicSqueal();
            var resolution = action.ResolveAttack(user, target2, gameState);

            // Verify 4 instances of Sonic Squeal
            Assert.NotNull(resolution);
            Assert.Equal(4, resolution.AttackInstances.Count);
            Assert.All(resolution.AttackInstances, hit =>
            {
                Assert.Equal("caveBat", hit.SourceCharacterId);
                Assert.Equal("Sonic Squeal", hit.ActionName);
                Assert.Equal(3, hit.BaseDamage);
                Assert.Equal(3, hit.FinalDamage);
            });
        }

        [Fact]
        // Test: Blood Peck deals 5 damage and heals for 3
        public void BloodPeckDealsDamageAndHeals()
        {
            // Initialize user and target for testing
            var user = new CaveBat() { Id = "caveBat", Health = 17, MaxHealth = 20 };
            var target = new Mage("player1") { Health = 6, MaxHealth = 30 };

            // Create a combat encounter with the user and target
            var gameState = new CombatEncounterGameState( new List<Character> { user }, new List<Character> { target });

            // Execute attack action
            var action = new BloodPeck();
            var resolution = action.ResolveAttack(user, target, gameState);

            // Verify a healing and attack instance was produced
            Assert.Single(resolution.AttackInstances);
            Assert.Single(resolution.HealInstances);

            var attack = resolution.AttackInstances[0];
            Assert.Equal("Blood Peck", attack.ActionName);
            Assert.Equal(user.Id, attack.SourceCharacterId);
            Assert.Equal(target.Id, attack.TargetCharacterId);
            Assert.Equal(5, attack.BaseDamage);
            Assert.Equal(5, attack.FinalDamage);

            var heal = resolution.HealInstances[0];
            Assert.Equal("Blood Peck", heal.ActionName);
            Assert.Equal(target.Id, heal.SourceCharacterId);
            Assert.Equal(user.Id, heal.TargetCharacterId);
            Assert.Equal(3, heal.BaseHealing);
            Assert.Equal(3, heal.FinalHealing);
        }
    }
}
