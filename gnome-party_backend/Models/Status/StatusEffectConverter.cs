using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Models.Status
{
    public class StatusEffectConverter : IPropertyConverter
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            WriteIndented = false
        };

        public DynamoDBEntry ToEntry(object value)
        {
            if (value is not StatusEffect statusEffect) { throw new ArgumentException("Expected a StatusEffect"); }
            string json = JsonSerializer.Serialize(statusEffect, typeof(StatusEffect), Options);
            return new Primitive(json);
        }

        public object FromEntry(DynamoDBEntry entry)
        {
            string json = entry.AsString();
            var statusEffect = JsonSerializer.Deserialize<StatusEffect>(json, Options)
                ?? throw new InvalidOperationException("Failed to deserialize StatusEffect");

            return statusEffect;
        }
    }
}