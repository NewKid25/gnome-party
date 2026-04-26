using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Models.CharacterData
{
    public class CharacterConverter : IPropertyConverter
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            WriteIndented = false
        };

        public DynamoDBEntry ToEntry(object value)
        {
            if (value is not Character character) { throw new ArgumentException("Expected a Character"); }
            string json = JsonSerializer.Serialize(character, typeof(Character), Options);
            return new Primitive(json);
        }

        public object FromEntry(DynamoDBEntry entry)
        {
            string json = entry.AsString();
            var character = JsonSerializer.Deserialize<Character>(json, Options)
                ?? throw new InvalidOperationException("Failed to deserialize Character");
            return character;
        }
    }
}