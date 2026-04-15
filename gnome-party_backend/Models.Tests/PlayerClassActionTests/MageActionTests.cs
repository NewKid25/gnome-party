using Models.Actions.PlayerClassActions.MageActions;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.Status;

namespace Models.Tests.PlayerClassActionTests
{
    public class MageActionTests
    {
        [Fact]
        // Test: Fireball hits target and burns target and adjacent allies
        public void FireballNormalCastHitsTargetAndBurnsTargetAndAdjacentAllies()
        {
            // Initialize caster and 3 enemies
            var caster = new Warrior("caster");
            var enemy1 = new Skeleton { Id = "enemy1" };
            var enemy2 = new Skeleton { Id = "enemy2" };
            var enemy3 = new Skeleton { Id = "enemy3" };
            var enemy4 = new Skeleton { Id = "enemy4" };

            // Create a combat encounter for testing
            var gameState = new CombatEncounterGameState(
                new List<Character> { caster },
                new List<Character> { enemy1, enemy2, enemy3 });

            // Execute Fireball action
            var action = new Fireball();
            var resolution = action.ResolveAttack(caster, enemy2, gameState, false);

            Assert.Single(resolution.AttackInstances); // Verify that Fireball processed

            // Verify Fireball physical damage
            var hit = resolution.AttackInstances[0];
            Assert.Equal("caster", hit.SourceCharacterId);
            Assert.Equal("enemy2", hit.TargetCharacterId);
            Assert.Equal("Fireball", hit.ActionName);
            Assert.Equal(6, hit.BaseDamage);
            Assert.False(hit.IsRedirected);

            // Verify 3 new instances of burn status and to the correct enemies
            Assert.Equal(3, resolution.StatusEffectsToApply.Count);
            var burnedIds = resolution.StatusEffectsToApply
                .Select(s => s.StatusOwnerCharacterId)
                .ToList();
            Assert.Contains("enemy1", burnedIds);
            Assert.Contains("enemy2", burnedIds);
            Assert.Contains("enemy3", burnedIds);
            Assert.DoesNotContain("enemy4", burnedIds);
            Assert.All(resolution.StatusEffectsToApply, status =>
            {
                Assert.IsType<BurnStatus>(status);
                Assert.Equal(3, status.Duration);
                Assert.Equal(DurationUnit.TurnStart, status.DurationUnit);
                Assert.Equal(2, (int)status.ModifierValues[StatusModifierKeys.TickDamage]);
            });
        }

        [Fact]
        // Test: Fireball redirected hits blocker and burns only the blocker
        public void FireballRedirectedHitsOnlyBlockerAndBurnsOnlyBlocker()
        {
            var caster = new Warrior("caster");
            var blocker = new Warrior("blocker");
            var ally1 = new Warrior("ally1");
            var ally2 = new Warrior("ally2");

            var gameState = new CombatEncounterGameState(
                new List<Character> { caster },
                new List<Character> { blocker, ally1, ally2 });

            var action = new Fireball();
            var resolution = action.ResolveAttack(caster, blocker, gameState, true);

            Assert.Single(resolution.AttackInstances);

            var hit = resolution.AttackInstances[0];
            Assert.Equal("blocker", hit.TargetCharacterId);
            Assert.Equal(6, hit.BaseDamage);
            Assert.True(hit.IsRedirected);

            Assert.Single(resolution.StatusEffectsToApply);
            var burn = resolution.StatusEffectsToApply[0];
            Assert.IsType<BurnStatus>(burn);
            Assert.Equal("blocker", burn.StatusOwnerCharacterId);
            Assert.Equal(3, burn.Duration);
            Assert.Equal(2, (int)burn.ModifierValues[StatusModifierKeys.TickDamage]);
        }

        [Fact]
        // Test: Ice Ray deals damage and applies chill status to target
        public void IceRayDealsDamageAppliesChillStatus()
        {
            // Initialize user and target for testing
            var user = new Mage("user");
            var target = new Skeleton { Id = "target" };

            // Create a combat encounter for testing
            var gameState = new CombatEncounterGameState(
                new List<Character> { user },
                new List<Character> { target });

            // Execute Ice Ray
            var action = new IceRay();
            var resolution = action.ResolveAttack(user, target, gameState);

            Assert.Single(resolution.AttackInstances); // Verify an attack resolution was created

            // Verify damage dealt and status applied
            var hit = resolution.AttackInstances[0];
            Assert.Equal("user", hit.SourceCharacterId);
            Assert.Equal("target", hit.TargetCharacterId);
            Assert.Equal("Ice Ray", hit.ActionName);
            Assert.Equal(5, hit.BaseDamage);
            Assert.Equal(5, hit.FinalDamage);

            // Verify Chill Status was applied correctly
            var status = resolution.StatusEffectsToApply[0];
            Assert.IsType<ChillStatus>(status);
            Assert.Equal("user", status.SourceCharacterId);
            Assert.Equal("target", status.StatusOwnerCharacterId);
            Assert.Equal(1, status.Duration);
            Assert.Equal(DurationUnit.TurnEnd, status.DurationUnit);
            Assert.Contains("target", status.AffectedCharacterIds);

            Assert.NotEmpty(resolution.StatusEffectsToApply);
        }

        [Fact]
        // Test: Magic Missle deals 10 damage to target
        public void MagicMissileDeals10DamageToTarget()
        {
            // Initialize user and target for testing
            var user = new Mage("user");
            var target = new Skeleton { Id = "target" };

            // Create an active combat encounter for testing
            var gameState = new CombatEncounterGameState(
                new List<Character> { user },
                new List<Character> { target });

            // Execute Magic Missile action
            var action = new MagicMisslie();
            var resolution = action.ResolveAttack(user, target, gameState, false, true);

            // Verify one attack resolution with the correct values
            Assert.Single(resolution.AttackInstances);
            var hit = resolution.AttackInstances[0];
            Assert.Equal("user", hit.SourceCharacterId);
            Assert.Equal("target", hit.TargetCharacterId);
            Assert.Equal("Magic Missile", hit.ActionName);
            Assert.Equal(10, hit.BaseDamage);
            Assert.Equal(10, hit.FinalDamage);

            Assert.Empty(resolution.StatusEffectsToApply); // Verify that no status has been applied
        }

        [Fact]
        // Test: Mirror applies the mirror status to the user
        public void MirrorAppliesMirrorStatusToUser()
        {
            // Initialize mage and enemy for testing
            var mage = new Mage("mage");
            var enemy = new Skeleton { Id = "enemy" };

            // Create a combat encounter for testing
            var gameState = new CombatEncounterGameState(
                new List<Character> { mage },
                new List<Character> { enemy });

            // Execute Mirror action
            var action = new Mirror();
            var resolution = action.ResolveAttack(mage, enemy, gameState);

            Assert.Empty(resolution.AttackInstances); // Verify no attack resolution was produced
            Assert.Single(resolution.StatusEffectsToApply); // Verify a new status has been applied

            // Verify that the correct status has been applied with the correct parameters
            var status = resolution.StatusEffectsToApply[0];
            Assert.IsType<MirrorStatus>(status);
            Assert.Equal("mage", status.SourceCharacterId);
            Assert.Equal("mage", status.StatusOwnerCharacterId);
            Assert.Equal(2, status.Duration);
            Assert.Equal(DurationUnit.TurnEnd, status.DurationUnit);
            Assert.Contains("enemy", status.AffectedCharacterIds);
        }
    }
}