using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Models.Status
{
    public class StatusEffectListConverter : IPropertyConverter
    {
        private static readonly StatusEffectConverter Inner = new();

        public DynamoDBEntry ToEntry(object value)
        {
            if (value is not List<StatusEffect> statusEffects) { throw new ArgumentException("Expected a List<StatusEffect>"); }
            var list = new DynamoDBList();
            foreach (var statusEffect in statusEffects) { list.Add(Inner.ToEntry(statusEffect)); }
            return list;
        }

        public object FromEntry(DynamoDBEntry entry)
        {
            if (entry is not DynamoDBList list) { throw new ArgumentException("Expected a DynamoDBList"); }
            return list.Entries.Select(e => (StatusEffect)Inner.FromEntry(e)).ToList();
        }
    }
}