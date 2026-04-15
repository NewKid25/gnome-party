using System.ComponentModel.DataAnnotations;
using Models.Actions.PlayerClassActions.BardActions;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.Status;
using static Models.CharacterData.PlayerCharacterClasses.Bard;

namespace Models.Tests.PlayerClassActionTests
{
    public class BardActionTests
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

        [Fact]
        // Test: Discord does 8 damage and resets the Bardic Song to "Soothing Song"
        public void DiscordChangesSongAndDealsDamage()
        {
            // Initialize bard and enemy for testing
            var bard = new Bard("bard");
            var enemy = new Skeleton { Id = "enemy" };

            // Create a combat encounter for testing
            var gameState = new CombatEncounterGameState(
                new List<Character> { bard },
                new List<Character> { enemy }
                );

            // Manually change bard song to test that Discord reset it
            bard.CurrentSong = "Frightening Song";

            // Execute Discord action
            var action = new Discord();
            var resolution = action.ResolveAttack(bard, enemy, gameState);

            Assert.Single(resolution.AttackInstances); // Verify an attack resolution was created

            // Verify damage dealt and song changed
            var hit = resolution.AttackInstances[0];
            Assert.Equal("bard", hit.SourceCharacterId);
            Assert.Equal("enemy", hit.TargetCharacterId);
            Assert.Equal("Discord", hit.ActionName);
            Assert.Equal(8, hit.BaseDamage);
            Assert.Equal(8, hit.FinalDamage);

            // Verify Bardic Song was reset correctly
            Assert.Equal("Soothing Song", bard.CurrentSong);
        }

        [Fact]
        // Test: Power Cord uses Soothing Song on all allies
        public void PowerCordSoothingSongOnAllAllies()
        {
            // Initialize characters for testing
            var bard = new Bard("bard");
            var warrior1 = new Warrior("warrior1") { Health = 14, MaxHealth = 30 }; 
            var warrior2 = new Warrior("warrior2") { Health = 20, MaxHealth = 30 };
            var warrior3 = new Warrior("warrior3") { Health = 20, MaxHealth = 30 };
            var enemy = new Skeleton() { Id = "skeleton", Health = 40, MaxHealth = 40 };
            var allies = new List<Character> { bard, warrior1, warrior2, warrior3 };

            var gameState = new CombatEncounterGameState( allies , new List<Character> { enemy }); // Initialize a gameState for testing

            // Choose Soothing Song manually then execute Power Cord
            bard.CurrentSong = BardSongs.Soothing;
            var action = new PowerCord();
            var resolution = action.ResolveAttack(bard, warrior2, gameState);

            // Verify no attack was registered but rather a healing instance
            Assert.Empty(resolution.AttackInstances);
            Assert.Equal(4, resolution.HealInstances.Count);

            // Verify that the healing affect went through
            var healTargetIds = resolution.HealInstances.Select(h => h.TargetCharacterId).ToList();
            Assert.Contains(bard.Id, healTargetIds);
            Assert.Contains(warrior1.Id, healTargetIds);
            Assert.Contains(warrior2.Id, healTargetIds);
            Assert.Contains(warrior3.Id, healTargetIds);
            Assert.All(resolution.HealInstances, heal => 
            {
                Assert.Equal(BardSongs.Soothing, heal.ActionName);
                Assert.Equal(bard.Id, heal.SourceCharacterId);
                Assert.Equal(8, heal.BaseHealing);
                Assert.Equal(8, heal.FinalHealing);
            });

            // Verify that a status was given (should be stun)
            Assert.NotEmpty(resolution.StatusEffectsToApply);
            var status = resolution.StatusEffectsToApply[0];
            Assert.IsType<StunStatus>(status);
            Assert.Equal("bard", status.SourceCharacterId);
            Assert.Equal("bard", status.StatusOwnerCharacterId);
            Assert.Equal(1, status.Duration);
            Assert.Equal(DurationUnit.TurnStart, status.DurationUnit);
        }

        [Fact]
        // Test: Power Cord uses Inspiring Song on all allies
        public void PowerCordInspiringOnAllAllies()
        {
            // Initialize characters for testing
            var bard = new Bard("bard");
            var warrior1 = new Warrior("warrior1") { Health = 14, MaxHealth = 30 };
            var warrior2 = new Warrior("warrior2") { Health = 20, MaxHealth = 30 };
            var warrior3 = new Warrior("warrior3") { Health = 20, MaxHealth = 30 };
            var enemy = new Skeleton() { Id = "skeleton", Health = 40, MaxHealth = 40 };
            var allies = new List<Character> { bard, warrior1, warrior2, warrior3 };

            var gameState = new CombatEncounterGameState(allies, new List<Character> { enemy }); // Initialize a gameState for testing

            // Choose Inspiring Song manually then execute Power Cord
            bard.CurrentSong = BardSongs.Inspiring;
            var action = new PowerCord();
            var resolution = action.ResolveAttack(bard, warrior2, gameState);

            // Verify that no attack or healing instance was generated
            Assert.Empty(resolution.AttackInstances);
            Assert.Empty(resolution.HealInstances);

            // Verify 4 new instances of Inspired Status (for the ally team)
            // 1 instance of stun against the Bard
            Assert.Equal(5, resolution.StatusEffectsToApply.Count);

            // Verify that the Bard was stunned after using Power Cord
            var stunStatus = resolution.StatusEffectsToApply.SingleOrDefault(s => s is StunStatus);
            Assert.NotNull(stunStatus);
            Assert.Equal(bard.Id, stunStatus!.SourceCharacterId);
            Assert.Equal(bard.Id, stunStatus.StatusOwnerCharacterId);
            Assert.Equal(1, stunStatus.Duration);
            Assert.Equal(DurationUnit.TurnStart, stunStatus.DurationUnit);

            // Verify 4 Inspired Statuses (1 for each member of the ally team)
            var inspiredTargets = resolution.StatusEffectsToApply.Where(s => s is InspiredStatus).Cast<InspiredStatus>().ToList();
            Assert.Equal(4, inspiredTargets.Count);
            var inspiredIds = inspiredTargets.Select(s => s.StatusOwnerCharacterId).ToList();
            Assert.Contains(bard.Id, inspiredIds);
            Assert.Contains(warrior1.Id, inspiredIds);
            Assert.Contains(warrior2.Id, inspiredIds);
            Assert.Contains(warrior3.Id, inspiredIds);
        }

        [Fact]
        // Test: Power Cord uses Frightening Song on all enemeis
        public void PowerCordFrighteningOnAllEnemies()
        {
            // Initialize characters for testing
            var bard = new Bard("bard");
            var enemy1 = new Skeleton() { Id = "skeleton1" };
            var enemy2 = new Skeleton() { Id = "skeleton2" };
            var enemy3 = new Skeleton() { Id = "skeleton3" };
            var enemy4 = new Skeleton() { Id = "skeleton4" };
            var enemies = new List<Character> { enemy1, enemy2, enemy3, enemy4 };

            var gameState = new CombatEncounterGameState(new List<Character> { bard }, enemies); // Initialize a gameState for testing

            // Chooose Frightening Song manually then execute Power Cord
            bard.CurrentSong = BardSongs.Frightening;
            var action = new PowerCord();
            var resolution = action.ResolveAttack(bard, enemy1, gameState);

            // Verify that no attack or healing instance was generated
            Assert.Empty(resolution.AttackInstances);
            Assert.Empty(resolution.HealInstances);

            // Verify 4 new instances of Stun Status (for the enemy team)
            // 1 instance of stun against the Bard
            Assert.Equal(5, resolution.StatusEffectsToApply.Count);

            // Verify 5 Stun Statuses (1 for each member of the enemies team and 1 for the bard)
            var stunTargets = resolution.StatusEffectsToApply.Where(s => s is StunStatus).Cast<StunStatus>().ToList();
            Assert.Equal(5, stunTargets.Count);
            var stunnedIds = stunTargets.Select(s => s.StatusOwnerCharacterId).ToList();
            Assert.Contains(bard.Id, stunnedIds);
            Assert.Contains(enemy1.Id, stunnedIds);
            Assert.Contains(enemy2.Id, stunnedIds);
            Assert.Contains(enemy3.Id, stunnedIds);
            Assert.Contains(enemy4.Id, stunnedIds);
        }

        [Fact]
        // Test: Song uses Soothing Song and heals
        public void SongUsesSoothingSongAndCyclesToInspiringSong()
        {
            // Initialize bard, ally, and enemy for testing
            var bard = new Bard("bard");
            var ally = new Warrior("ally")
            {
                Health = 10,
                MaxHealth = 30
            };
            var enemy = new Skeleton { Id = "enemy" };

            // Initialize a gameState for testing
            var gameState = new CombatEncounterGameState(
                new List<Character> { bard, ally },
                new List<Character> { enemy });

            bard.CurrentSong = BardSongs.Soothing; // Get the current bardic song

            // Execute the Song action
            var action = new Song();
            var resolution = action.ResolveAttack(bard, ally, gameState);

            // Make sure an instance of healing was returned instead of an attack instance
            Assert.Empty(resolution.AttackInstances);
            Assert.Single(resolution.HealInstances);

            // Verify the Soothing Song parameters
            var heal = resolution.HealInstances[0];
            Assert.Equal("Song", action.AttackName);
            Assert.Equal("Soothing Song", heal.ActionName);
            Assert.Equal(bard.Id, heal.SourceCharacterId);
            Assert.Equal(ally.Id, heal.TargetCharacterId);
            Assert.Equal(8, heal.BaseHealing);
            Assert.Equal(8, heal.FinalHealing);

            Assert.Equal(BardSongs.Inspiring, bard.CurrentSong); // Verify that the current bardic song has changed
        }

        [Fact]
        // Test: Song uses Inspiring Song and applies Inspiring Song Status
        public void SongUsesInspiringSongAndCyclesToFrightening()
        {
            var bard = new Bard("bard");
            var ally = new Warrior("ally");
            var enemy = new Skeleton { Id = "enemy" };

            var gameState = new CombatEncounterGameState(
                new List<Character> { bard, ally },
                new List<Character> { enemy });

            bard.CurrentSong = BardSongs.Inspiring;

            var action = new Song();
            var resolution = action.ResolveAttack(bard, ally, gameState);

            Assert.Empty(resolution.AttackInstances);
            Assert.Empty(resolution.HealInstances);
            Assert.Single(resolution.StatusEffectsToApply);

            var status = resolution.StatusEffectsToApply[0];
            Assert.IsType<InspiredStatus>(status);
            Assert.Equal(bard.Id, status.SourceCharacterId);
            Assert.Equal(ally.Id, status.StatusOwnerCharacterId);
            Assert.Equal(1, status.Duration);
            Assert.Equal(DurationUnit.TurnEnd, status.DurationUnit);

            Assert.Equal(BardSongs.Frightening, bard.CurrentSong);
        }

        [Fact]
        // Test: Song uses Frightening Song and stuns the enemy
        public void SongUsesFrighteningSongStunsEnemyAndCyclesToSoothing()
        {
            var bard = new Bard("bard");
            var ally = new Warrior("ally");
            var enemy = new Skeleton { Id = "enemy" };

            var gameState = new CombatEncounterGameState(
                new List<Character> { bard, ally },
                new List<Character> { enemy });

            bard.CurrentSong = BardSongs.Frightening;

            var action = new Song();
            var resolution = action.ResolveAttack(bard, enemy, gameState);

            Assert.Empty(resolution.AttackInstances);
            Assert.Empty(resolution.HealInstances);
            Assert.Single(resolution.StatusEffectsToApply);

            var status = resolution.StatusEffectsToApply[0];
            Assert.IsType<StunStatus>(status);
            Assert.Equal(enemy.Id, status.SourceCharacterId);
            Assert.Equal(enemy.Id, status.StatusOwnerCharacterId);
            Assert.Equal(1, status.Duration);
            Assert.Equal(DurationUnit.TurnStart, status.DurationUnit);

            Assert.Equal(BardSongs.Soothing, bard.CurrentSong);
        }
    }
}
