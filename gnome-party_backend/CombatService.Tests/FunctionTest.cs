using Xunit;
using Amazon.Lambda.TestUtilities;
using System.Text.Json;
using System.Text.Json.Serialization;
using CombatService;

namespace CombatService.Tests
{
    public class FunctionTest
    {
        private JsonSerializerOptions BuildOptions()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        [Fact]
        public void Warrior_Slash_Mage_Reduces_Health()
        {
            var function = new Function();
            var context = new TestLambdaContext();

            string input = @"
            {
                ""Attacker"": ""Warrior"",
                ""Target"": ""Mage"",
                ""Attack"": ""Slash""
            }";

            string resultJson = function.FunctionHandler(input, context);

            CombatResult result = JsonSerializer.Deserialize<CombatResult>(resultJson, BuildOptions());

            Assert.NotNull(result);
            Assert.Equal("Warrior", result.AttackerName);
            Assert.Equal("Mage", result.TargetName);
            Assert.Equal(10, result.TargetHealth);
            Assert.NotEqual(System.Guid.Empty, result.AttackerId);
            Assert.NotEqual(System.Guid.Empty, result.TargetId);
        }

        [Fact]
        public void Invalid_Attack_Returns_Error()
        {
            var function = new Function();
            var context = new TestLambdaContext();

            string input = @"
            {
                ""Attacker"": ""Warrior"",
                ""Target"": ""Mage"",
                ""Attack"": ""Fireball""
            }";

            string result = function.FunctionHandler(input, context);

            Assert.Equal("Invalid attack", result);
        }

        [Fact]
        public void Invalid_Json_Returns_Error()
        {
            var function = new Function();
            var context = new TestLambdaContext();

            string input = "{ bad json }";

            string result = function.FunctionHandler(input, context);

            Assert.Equal("Invalid JSON input", result);
        }

        [Fact]
        public void Invalid_Character_Returns_Error()
        {
            var function = new Function();
            var context = new TestLambdaContext();

            string input = @"
            {
                ""Attacker"": 999,
                ""Target"": ""Mage"",
                ""Attack"": ""Slash""
            }";

            string result = function.FunctionHandler(input, context);

            Assert.Equal("Invalid character type", result);
        }
    }
    public class BlockTests
    {
        [Fact]
        public void Block_Redirects_Attack_To_Protector_And_Reduces_Damage()
        {
            // Arrange
            var protector = new Warrior("Protector", Guid.NewGuid()); // starts 30 HP
            var protectedTarget = new Mage("Mage", Guid.NewGuid());   // starts 20 HP
            var attacker = new Warrior("Attacker", Guid.NewGuid());   // has Slash

            // Protector uses Block on Mage
            protector.ResolveAttack(new Block(), protectedTarget);

            // Act: Attacker tries to Slash the Mage
            attacker.ResolveAttack(new Slash(), protectedTarget);

            // Assert:
            // - Mage should take 0 because redirect sends attack to protector
            Assert.Equal(20, protectedTarget.Health);

            // - Protector should take 5 damage (10 reduced by 50%)
            Assert.Equal(25, protector.Health);
        }

        [Fact]
        public void Block_Adds_Statuses_To_Correct_Characters()
        {
            // Arrange
            var protector = new Warrior("Protector", Guid.NewGuid());
            var protectedTarget = new Mage("Mage", Guid.NewGuid());

            // Act
            protector.ResolveAttack(new Block(), protectedTarget);

            // Assert: protected target should have redirect status
            bool hasRedirect = false;
            foreach (var s in protectedTarget.Statuses)
            {
                if (s is RedirectAttackToCaster_Status)
                {
                    hasRedirect = true;
                    break;
                }
            }
            Assert.True(hasRedirect);

            // Assert: protector should have damage reduction status
            bool hasReduction = false;
            foreach (var s in protector.Statuses)
            {
                if (s is DamageReduction_Status)
                {
                    hasReduction = true;
                    break;
                }
            }
            Assert.True(hasReduction);
        }

        [Fact]
        public void Block_Effects_Expire_At_Start_Of_Protector_Turn()
        {
            // Arrange
            var protector = new Warrior("Protector", Guid.NewGuid());
            var protectedTarget = new Mage("Mage", Guid.NewGuid());
            var attacker = new Warrior("Attacker", Guid.NewGuid());

            protector.ResolveAttack(new Block(), protectedTarget);

            // Start of protector's turn: statuses that expire for protector should be removed
            protector.RemoveExpiredStatusesAtStartOfTurn(protector.CharacterID);
            protectedTarget.RemoveExpiredStatusesAtStartOfTurn(protector.CharacterID);

            // Act: attacker slashes mage again
            attacker.ResolveAttack(new Slash(), protectedTarget);

            // Assert:
            // Redirect + reduction should be gone now
            // Mage takes full 10
            Assert.Equal(10, protectedTarget.Health);

            // Protector takes no damage this time (should still be 30)
            Assert.Equal(30, protector.Health);
        }
    }
}