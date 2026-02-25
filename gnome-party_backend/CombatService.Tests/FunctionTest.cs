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
    }
}