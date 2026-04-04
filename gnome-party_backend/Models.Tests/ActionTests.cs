using System.Net.NetworkInformation;
using CombatService;
using Models.Actions;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.EncounterData;
using Models.Status;

namespace Models.Tests
{
    public class ActionTests
    {
        [Fact]
        public void SlashDeals10DamageToTarget()
        {
            var user = new Warrior("user");
            var target = new Skeleton { Id = "target" };
            var gameState = new CombatEncounterGameState(
                new List<Character> { user },
                new List<Character> { target });

            var action = new Slash();
            var resolution = action.ResolveAttack(user, target, gameState);

            Assert.Single(resolution.AttackInstances);

            var hit = resolution.AttackInstances[0];
            Assert.Equal("user", hit.SourceCharacterId);
            Assert.Equal("target", hit.TargetCharacterId);
            Assert.Equal("Slash", hit.ActionName);
            Assert.Equal(10, hit.BaseDamage);
            Assert.Equal(10, hit.FinalDamage);

            Assert.Empty(resolution.StatusEffectsToApply);
        }
        [Fact]
        public void BoneSlashDeals6DamageToTarget()
        {
            var user = new Skeleton { Id = "enemy" };
            var target = new Warrior("player");
            var gameState = new CombatEncounterGameState(
                new List<Character> { target },
                new List<Character> { user });

            var action = new BoneSlash();
            var resolution = action.ResolveAttack(user, target, gameState);

            Assert.Single(resolution.AttackInstances);

            var hit = resolution.AttackInstances[0];
            Assert.Equal("enemy", hit.SourceCharacterId);
            Assert.Equal("player", hit.TargetCharacterId);
            Assert.Equal("Bone Slash", hit.ActionName);
            Assert.Equal(6, hit.BaseDamage);
            Assert.Equal(6, hit.FinalDamage);

            Assert.Empty(resolution.StatusEffectsToApply);
        }
        [Fact]
        public void BlockAppliesBlockStatusToUserProtectingTarget()
        {
            var user = new Warrior("blocker");
            var ally = new Warrior("ally");
            var gameState = new CombatEncounterGameState(
                new List<Character> { user, ally },
                new List<Character>());

            var action = new Block();
            var resolution = action.ResolveAttack(user, ally, gameState);

            Assert.Empty(resolution.AttackInstances);
            Assert.Single(resolution.StatusEffectsToApply);

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
        public void FuryStrikesWith2HitsCreatesTwoAttacks()
        {
            var user = new Warrior("user");
            var target = new Skeleton { Id = "target" };
            var gameState = new CombatEncounterGameState(
                new List<Character> { user },
                new List<Character> { target });

            var action = new FuryStrikes(2);
            var resolution = action.ResolveAttack(user, target, gameState);

            Assert.Equal(2, resolution.AttackInstances.Count);

            Assert.All(resolution.AttackInstances, hit =>
            {
                Assert.Equal("user", hit.SourceCharacterId);
                Assert.Equal("target", hit.TargetCharacterId);
                Assert.Equal("Fury Strikes", hit.ActionName);
                Assert.Equal(3, hit.BaseDamage);
                Assert.Equal(3, hit.FinalDamage);
            });

            Assert.Empty(resolution.StatusEffectsToApply);
        }
        [Fact]
        public void FuryStrikesWith4HitsCreatesFourAttacks()
        {
            var user = new Warrior("user");
            var target = new Skeleton { Id = "target" };
            var gameState = new CombatEncounterGameState(
                new List<Character> { user },
                new List<Character> { target });

            var action = new FuryStrikes(4);
            var resolution = action.ResolveAttack(user, target, gameState);

            Assert.Equal(4, resolution.AttackInstances.Count);

            Assert.All(resolution.AttackInstances, hit =>
            {
                Assert.Equal("target", hit.TargetCharacterId);
                Assert.Equal(3, hit.BaseDamage);
            });
        }
        [Fact]
        public void FireballNormalCastHitsTargetAndBurnsTargetAndAdjacentAllies()
        {
            var caster = new Warrior("caster");

            var enemy1 = new Skeleton { Id = "enemy1" };
            var enemy2 = new Skeleton { Id = "enemy2" };
            var enemy3 = new Skeleton { Id = "enemy3" };

            var gameState = new CombatEncounterGameState(
                new List<Character> { caster },
                new List<Character> { enemy1, enemy2, enemy3 });

            var action = new Fireball();
            var resolution = action.ResolveAttack(caster, enemy2, gameState, false);

            Assert.Single(resolution.AttackInstances);

            var hit = resolution.AttackInstances[0];
            Assert.Equal("caster", hit.SourceCharacterId);
            Assert.Equal("enemy2", hit.TargetCharacterId);
            Assert.Equal("Fireball", hit.ActionName);
            Assert.Equal(6, hit.BaseDamage);
            Assert.False(hit.IsRedirected);

            Assert.Equal(3, resolution.StatusEffectsToApply.Count);

            var burnedIds = resolution.StatusEffectsToApply
                .Select(s => s.StatusOwnerCharacterId)
                .ToList();

            Assert.Contains("enemy1", burnedIds);
            Assert.Contains("enemy2", burnedIds);
            Assert.Contains("enemy3", burnedIds);

            Assert.All(resolution.StatusEffectsToApply, status =>
            {
                Assert.IsType<BurnStatus>(status);
                Assert.Equal(3, status.Duration);
                Assert.Equal(DurationUnit.TurnStart, status.DurationUnit);
                Assert.Equal(2, (int)status.ModifierValues[StatusModifierKeys.TickDamage]);
            });
        }
        [Fact]
        public void FireballRedirectedHitsOnlyBlockerAndBurnsOnlyBlocker()
        {
            var caster = new Warrior("caster");
            var blocker = new Warrior("blocker");
            var ally1 = new Warrior("ally1");
            var ally2 = new Warrior("ally2");

            var gameState = new CombatEncounterGameState(
                new List<Character> { blocker, ally1, ally2, caster },
                new List<Character>());

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
        public void RattleGuardAppliesSelfDamageReductionStatus()
        {
            var user = new Skeleton { Id = "skeleton1", Name = "Skeleton" };
            var gameState = new CombatEncounterGameState(
                new List<Character>(),
                new List<Character> { user });

            var action = new RattleGuard();
            var resolution = action.ResolveAttack(user, user, gameState);

            Assert.Empty(resolution.AttackInstances);
            Assert.Single(resolution.StatusEffectsToApply);

            var status = resolution.StatusEffectsToApply[0];
            Assert.IsType<RattleGuardStatus>(status);
            Assert.Equal("skeleton1", status.SourceCharacterId);
            Assert.Equal("skeleton1", status.StatusOwnerCharacterId);
            Assert.Equal(1, status.Duration);
            Assert.Equal(DurationUnit.TurnStart, status.DurationUnit);
            Assert.Equal(0.5, status.ModifierValues[StatusModifierKeys.DamageReduction]);
        }
        [Fact]
        public void ParryAppliesParryStatusToUserAgainstTargetEnemy()
        {
            var user = new Warrior("user");
            var enemy = new Skeleton { Id = "enemy" };
            var gameState = new CombatEncounterGameState(
                new List<Character> { user },
                new List<Character> { enemy });

            var action = new Parry();
            var resolution = action.ResolveAttack(user, enemy, gameState);

            Assert.Empty(resolution.AttackInstances);
            Assert.Single(resolution.StatusEffectsToApply);

            var status = resolution.StatusEffectsToApply[0];
            Assert.IsType<ParryStatus>(status);
            Assert.Equal("user", status.SourceCharacterId);
            Assert.Equal("user", status.StatusOwnerCharacterId);
            Assert.Equal(1, status.Duration);
            Assert.Equal(DurationUnit.TurnStart, status.DurationUnit);
            Assert.Contains("enemy", status.AffectedCharacterIds);
        }
        [Fact]
        public void MagicMissileDeals10DamageToTarget()
        {
            bool isRedirected = false;
            bool unblockable = true;
            var user = new Mage("user");
            var target = new Skeleton { Id = "target" };
            var gameState = new CombatEncounterGameState(
                new List<Character> { user },
                new List<Character> { target });

            var action = new MagicMisslie();
            var resolution = action.ResolveAttack(user, target, gameState, isRedirected, unblockable);

            Assert.Single(resolution.AttackInstances);

            var hit = resolution.AttackInstances[0];
            Assert.Equal("user", hit.SourceCharacterId);
            Assert.Equal("target", hit.TargetCharacterId);
            Assert.Equal("Magic Missile", hit.ActionName);
            Assert.Equal(10, hit.BaseDamage);
            Assert.Equal(10, hit.FinalDamage);

            Assert.Empty(resolution.StatusEffectsToApply);
        }
        [Fact]
        public void MirrorAppliesMirrorStatusToUser()
        {
            var mage = new Mage("mage");
            var enemy = new Skeleton { Id = "enemy" };
            var gameState = new CombatEncounterGameState(
                new List<Character> { mage },
                new List<Character> { enemy });
            var action = new Mirror();
            var resolution = action.ResolveAttack(mage, enemy, gameState);

            Assert.Empty(resolution.AttackInstances);
            Assert.Single(resolution.StatusEffectsToApply);

            var status = resolution.StatusEffectsToApply[0];
            Assert.IsType<MirrorStatus>(status);
            Assert.Equal("mage", status.SourceCharacterId);
            Assert.Equal("mage", status.StatusOwnerCharacterId);
            Assert.Equal(2, status.Duration);
            Assert.Equal(DurationUnit.TurnEnd, status.DurationUnit);
            Assert.Contains("enemy", status.AffectedCharacterIds);

        }
        [Fact]
        public void IceRayDealsDamageAppliesChillStatus()
        {
            var user = new Mage("user");
            var target = new Skeleton { Id = "target" };
            var gameState = new CombatEncounterGameState(
                new List<Character> { user },
                new List<Character> { target });

            var action = new IceRay();
            var resolution = action.ResolveAttack(user, target, gameState);

            Assert.Single(resolution.AttackInstances);

            var hit = resolution.AttackInstances[0];
            Assert.Equal("user", hit.SourceCharacterId);
            Assert.Equal("target", hit.TargetCharacterId);
            Assert.Equal("Ice Ray", hit.ActionName);
            Assert.Equal(5, hit.BaseDamage);
            Assert.Equal(5, hit.FinalDamage);

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
        public void WhirlingStrikeDamagesAllEnemies()
        {
            var warrior = new Warrior("warrior");

            var enemy1 = new Skeleton { Id = "enemy1" };
            var enemy2 = new Skeleton { Id = "enemy2" };
            var enemy3 = new Skeleton { Id = "enemy3" };
            var enemy4 = new Skeleton { Id = "enemy4" };
            var enemy5 = new Skeleton { Id = "enemy5" };
            var enemy6 = new Skeleton { Id = "enemy6" };

            var enemies = new List<Character> { enemy1, enemy2, enemy3, enemy4, enemy5, enemy6 };
            var gameState = new CombatEncounterGameState(new List<Character> { warrior }, enemies);

            var action = new WhirlingStrike();
            var resolution = action.ResolveAttack(warrior, enemy1, gameState, false);

            Assert.Equal(6, resolution.AttackInstances.Count);
            Assert.All(resolution.AttackInstances, hit =>
            {
                Assert.Equal("warrior", hit.SourceCharacterId);
                Assert.Equal("Whirling Strike", hit.ActionName);
                Assert.Equal(5, hit.BaseDamage);
                Assert.Equal(5, hit.FinalDamage);
            });

            var targetIds = resolution.AttackInstances.Select(hit => hit.TargetCharacterId).ToList();
            Assert.Equal(6, targetIds.Distinct().Count());
            
            var expectedIds = enemies.Select(e => e.Id).OrderBy(id => id).ToList();
            var actualIds = targetIds.OrderBy(id => id).ToList();
            Assert.Equal(expectedIds, actualIds);

            Assert.Empty(resolution.StatusEffectsToApply);
        }
    }
}