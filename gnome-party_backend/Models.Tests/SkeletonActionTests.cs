using Models.Actions.SkeletonActions;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.Status;

namespace Models.Tests
{
    public class SkeletonActionTests
    {
        [Fact]
        // Test: Bone Slash damages target for 6
        public void BoneSlashDeals6DamageToTarget()
        {
            // Insitialize user and target for testing
            var user = new Skeleton { Id = "enemy" };
            var target = new Warrior("player");

            // Create a combat encounter with user and target
            var gameState = new CombatEncounterGameState(
                new List<Character> { target },
                new List<Character> { user });

            // Execute attack action
            var action = new BoneSlash();
            var resolution = action.ResolveAttack(user, target, gameState);

            Assert.Single(resolution.AttackInstances); // Verify a single attack resolution was passed

            // Verify the attack resolution passed back the correct things
            var hit = resolution.AttackInstances[0];
            Assert.Equal("enemy", hit.SourceCharacterId);
            Assert.Equal("player", hit.TargetCharacterId);
            Assert.Equal("Bone Slash", hit.ActionName);
            Assert.Equal(6, hit.BaseDamage);
            Assert.Equal(6, hit.FinalDamage);

            Assert.Empty(resolution.StatusEffectsToApply); // Verify that no status effects were applied
        }

        [Fact]
        // Test: Rattle Guard applies self damage reduction status
        public void RattleGuardAppliesSelfDamageReductionStatus()
        {
            // Initialize user for testing
            var user = new Skeleton { Id = "skeleton1", Name = "Skeleton" };

            // Create a combat encounter for testing
            var gameState = new CombatEncounterGameState(
                new List<Character>(),
                new List<Character> { user });

            // Exeecute Rattle Guard action
            var action = new RattleGuard();
            var resolution = action.ResolveAttack(user, user, gameState);

            // Verify no attack instances were created and a status was applied
            Assert.Empty(resolution.AttackInstances);
            Assert.Single(resolution.StatusEffectsToApply);

            // Verify correct status and details
            var status = resolution.StatusEffectsToApply[0];
            Assert.IsType<RattleGuardStatus>(status);
            Assert.Equal("skeleton1", status.SourceCharacterId);
            Assert.Equal("skeleton1", status.StatusOwnerCharacterId);
            Assert.Equal(1, status.Duration);
            Assert.Equal(DurationUnit.TurnStart, status.DurationUnit);
            Assert.Equal(0.5, status.ModifierValues[StatusModifierKeys.DamageReduction]);
        }
    }
}