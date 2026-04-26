using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Models.CharacterData
{
    public class CharacterListConverter : IPropertyConverter
    {
        private static readonly CharacterConverter Inner = new();
        public DynamoDBEntry ToEntry(object value)
        {
            if (value is not List<Character> characters) { throw new ArgumentException("Expected a List<Character>"); }
            var list = new DynamoDBList();
            foreach (var character in characters) { list.Add(Inner.ToEntry(character)); }
            return list;
        }

        public object FromEntry(DynamoDBEntry entry)
        {
            if (entry is not DynamoDBList list) { throw new ArgumentException("Expected a DynamoDBList"); }
            return list.Entries.Select(e => (Character)Inner.FromEntry(e)).ToList();
        }
    }
}