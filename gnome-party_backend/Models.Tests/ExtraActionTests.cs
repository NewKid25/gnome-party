using Models.Actions;
using Models.Actions.BardActions;
using Models.Actions.MageActions;
using Models.Actions.SkeletonActions;
using Models.Actions.WarriorActions;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.Status;

namespace Models.Tests
{
    public class ExtraActionTests
    {

        [Fact]
        // Test: Fury Strikes (2) creates two of the same attack instances targeting the same enemy
        public void FuryStrikesWith2HitsCreatesTwoAttacks()
        {
            // Initialize user and target for testing 
            var user = new Warrior("user");
            var target = new Skeleton { Id = "target" };
            
            // Create a combat encounter with user and target
            var gameState = new CombatEncounterGameState(
                new List<Character> { user },
                new List<Character> { target });

            // Implement FuryStrikes with 2 hits
            var action = new FuryStrikes(2);
            var resolution = action.ResolveAttack(user, target, gameState);

            // Verify that 2 attack instances were made and that they have the correct data
            Assert.Equal(2, resolution.AttackInstances.Count); 
            Assert.All(resolution.AttackInstances, hit =>
            {
                Assert.Equal("user", hit.SourceCharacterId);
                Assert.Equal("target", hit.TargetCharacterId);
                Assert.Equal("Fury Strikes", hit.ActionName);
                Assert.Equal(3, hit.BaseDamage);
                Assert.Equal(3, hit.FinalDamage);
            });

            Assert.Empty(resolution.StatusEffectsToApply); // Verify that no status effects were applied
        }

        [Fact]
        // Test: Fury Strikes (4) creates four of the same attack instances targeting the same enemy 
        public void FuryStrikesWith4HitsCreatesFourAttacks()
        {
            // Initialize the user and target for testing
            var user = new Warrior("user");
            var target = new Skeleton { Id = "target" };

            // Create a combat encounter for testing
            var gameState = new CombatEncounterGameState(
                new List<Character> { user },
                new List<Character> { target });

            // Generate Fury Strikes with 4 instances
            var action = new FuryStrikes(4);
            var resolution = action.ResolveAttack(user, target, gameState);

            // Verify that 4 attack instances were created and have the correct data
            Assert.Equal(4, resolution.AttackInstances.Count);
            Assert.All(resolution.AttackInstances, hit =>
            {
                Assert.Equal("target", hit.TargetCharacterId);
                Assert.Equal(3, hit.BaseDamage);
            });
        }
    }
}