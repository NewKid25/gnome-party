using Models.Actions.BardActions;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.Status;

namespace Models.Tests
{
    public class BardctionTests
    {
        [Fact]
        // Test: Mockery does 6 damage and applies Mock Status to user with enemy being the affected character
        public void MockeryAppliesMockAndDealsDamage()
        {
            // Initialize bard and enemy for testing
            var bard = new Bard("bard");
            var enemy = new Skeleton { Id = "enemy" };

            // Create a combat encounter for testing
            var gameState = new CombatEncounterGameState(
                new List<Character> { bard },
                new List<Character> { enemy });

            // Execute Mockery action
            var action = new Mockery();
            var resolution = action.ResolveAttack(bard, enemy, gameState);

            Assert.Single(resolution.AttackInstances); // Verify an attack resolution was created

            // Verify damage dealt, status applied, and affected character ID has the enemy
            var hit = resolution.AttackInstances[0];
            Assert.Equal("bard", hit.SourceCharacterId);
            Assert.Equal("enemy", hit.TargetCharacterId);
            Assert.Equal("Mockery", hit.ActionName);
            Assert.Equal(6, hit.BaseDamage);
            Assert.Equal(6, hit.FinalDamage);

            // Verify Mock Status was applied correctly
            var status = resolution.StatusEffectsToApply[0];
            Assert.IsType<MockStatus>(status);
            Assert.Equal("bard", status.SourceCharacterId);
            Assert.Equal("bard", status.StatusOwnerCharacterId);
            Assert.Equal(2, status.Duration);
            Assert.Equal(DurationUnit.TurnEnd, status.DurationUnit);
            Assert.Contains("enemy", status.AffectedCharacterIds);
            Assert.NotEmpty(resolution.StatusEffectsToApply);
        }
    }
}
