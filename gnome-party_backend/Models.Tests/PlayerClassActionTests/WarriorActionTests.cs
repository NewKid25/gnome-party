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

namespace Models.Tests.PlayerClassActionTests
{
    public class WarriorActionTests
    {
        [Fact]
        // Test: Block applies Block Status 
        public void BlockAppliesBlockStatusToUserProtectingTarget()
        {
            // Insitialize user and ally for testing
            var user = new Warrior("blocker");
            var ally = new Warrior("ally");

            // Create a combat encounter with user and ally for testing
            var gameState = new CombatEncounterGameState(
                new List<Character> { user, ally },
                new List<Character>());

            // Execute block action
            var action = new Block();
            var resolution = action.ResolveAttack(user, ally, gameState);

            Assert.Empty(resolution.AttackInstances); // Verify that no attack instance was produced
            Assert.Single(resolution.StatusEffectsToApply); // Verify that only the Block Status was produced

            // Verify the Block Status was applied correctly
            var status = resolution.StatusEffectsToApply[0];
            Assert.IsType<BlockStatus>(status);
            Assert.Equal("blocker", status.SourceCharacterId);
            Assert.Equal("blocker", status.StatusOwnerCharacterId);
            Assert.Equal(1, status.Duration);
            Assert.Equal(DurationUnit.TurnStart, status.DurationUnit);
            Assert.Contains("ally", status.AffectedCharacterIds);
            Assert.Equal(0.5, status.ModifierValues[StatusModifierKeys.DamageReduction]);
        }

        [Fact]
        // Test: Parry applies the parry status to the user when countering an enemy
        public void ParryAppliesParryStatusToUserAgainstTargetEnemy()
        {
            // Initialize user and enemy for testing
            var user = new Warrior("user");
            var enemy = new Skeleton { Id = "enemy" };

            // Create a combat encounter for testing
            var gameState = new CombatEncounterGameState(
                new List<Character> { user },
                new List<Character> { enemy });

            // Execute Parry
            var action = new Parry();
            var resolution = action.ResolveAttack(user, enemy, gameState);

            // Verify no attack instances and a status was applied
            Assert.Empty(resolution.AttackInstances);
            Assert.Single(resolution.StatusEffectsToApply);

            // Verify the correct status was applied
            var status = resolution.StatusEffectsToApply[0];
            Assert.IsType<ParryStatus>(status);
            Assert.Equal("user", status.SourceCharacterId);
            Assert.Equal("user", status.StatusOwnerCharacterId);
            Assert.Equal(1, status.Duration);
            Assert.Equal(DurationUnit.TurnStart, status.DurationUnit);
            Assert.Contains("enemy", status.AffectedCharacterIds);
        }

        [Fact]
        // Test: Slash damaged target for 10
        public void SlashDeals10DamageToTarget()
        {
            // Initialize user and target for testing
            var user = new Warrior("user");
            var target = new Skeleton { Id = "target" };

            // Create a combat encounter for testing
            var gameState = new CombatEncounterGameState(
                new List<Character> { user },
                new List<Character> { target });

            // Execute attack action
            var action = new Slash();
            var resolution = action.ResolveAttack(user, target, gameState);

            Assert.Single(resolution.AttackInstances); // Verify single attack instance

            // Verify the attack resolution passed back the correct things
            var hit = resolution.AttackInstances[0];
            Assert.Equal("user", hit.SourceCharacterId);
            Assert.Equal("target", hit.TargetCharacterId);
            Assert.Equal("Slash", hit.ActionName);
            Assert.Equal(10, hit.BaseDamage);
            Assert.Equal(10, hit.FinalDamage);

            Assert.Empty(resolution.StatusEffectsToApply); // Verify that no status was applied
        }

        [Fact]
        // Test: Whirling Strike deals damage to the entire enemy team
        public void WhirlingStrikeDamagesAllEnemies()
        {
            // Initialize warrior and 6 enemies 
            var warrior = new Warrior("warrior");
            var enemy1 = new Skeleton { Id = "enemy1" };
            var enemy2 = new Skeleton { Id = "enemy2" };
            var enemy3 = new Skeleton { Id = "enemy3" };
            var enemy4 = new Skeleton { Id = "enemy4" };
            var enemy5 = new Skeleton { Id = "enemy5" };
            var enemy6 = new Skeleton { Id = "enemy6" };

            // Consolidate each enemy into a list and make a new combat encounter for testing
            var enemies = new List<Character> { enemy1, enemy2, enemy3, enemy4, enemy5, enemy6 };
            var gameState = new CombatEncounterGameState(new List<Character> { warrior }, enemies);

            // Execute Whirling Strike
            var action = new WhirlingStrike();
            var resolution = action.ResolveAttack(warrior, enemy1, gameState, false);

            // Verify 6 attacks were made with the correct values
            Assert.Equal(6, resolution.AttackInstances.Count);
            Assert.All(resolution.AttackInstances, hit =>
            {
                Assert.Equal("warrior", hit.SourceCharacterId);
                Assert.Equal("Whirling Strike", hit.ActionName);
                Assert.Equal(5, hit.BaseDamage);
                Assert.Equal(5, hit.FinalDamage);
            });

            // Verify that all enemies were attacked
            var targetIds = resolution.AttackInstances.Select(hit => hit.TargetCharacterId).ToList();
            Assert.Equal(6, targetIds.Distinct().Count());
            var expectedIds = enemies.Select(e => e.Id).OrderBy(id => id).ToList();
            var actualIds = targetIds.OrderBy(id => id).ToList();
            Assert.Equal(expectedIds, actualIds);

            Assert.Empty(resolution.StatusEffectsToApply); // Verify that no status was applied
        }
    }
}
