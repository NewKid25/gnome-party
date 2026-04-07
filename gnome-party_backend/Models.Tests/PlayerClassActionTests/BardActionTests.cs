using Models.Actions.BardActions;
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
            Assert.IsType<InspiringSongStatus>(status);
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
