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
            // Initialize caster and blocker and 2 allies for testing
            var caster = new Warrior("caster");
            var blocker = new Warrior("blocker");
            var ally1 = new Warrior("ally1");
            var ally2 = new Warrior("ally2");

            // Create a combat encounter for testing
            var gameState = new CombatEncounterGameState(
                new List<Character> { blocker, ally1, ally2, caster },
                new List<Character>());

            // Execute Fireball with redirection
            var action = new Fireball();
            var resolution = action.ResolveAttack(caster, blocker, gameState, true);

            Assert.Single(resolution.AttackInstances); // Verify attack instance occured

            // Verify Fireball redirected damage
            var hit = resolution.AttackInstances[0];
            Assert.Equal("blocker", hit.TargetCharacterId);
            Assert.Equal(6, hit.BaseDamage);
            Assert.True(hit.IsRedirected);

            // Verify that only the blocker was burned
            Assert.Single(resolution.StatusEffectsToApply); 
            var burn = resolution.StatusEffectsToApply[0];
            Assert.IsType<BurnStatus>(burn);
            Assert.Equal("blocker", burn.StatusOwnerCharacterId);
            Assert.Equal(3, burn.Duration);
            Assert.Equal(2, (int)burn.ModifierValues[StatusModifierKeys.TickDamage]);
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