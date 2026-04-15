using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Models.EncounterData;

namespace Models;

public class EncounterListConverter : IPropertyConverter
{
    private static readonly EncounterConverter Inner = new();

    public DynamoDBEntry ToEntry(object value)
    {
        var encounters = (List<Encounter>)value;
        var list = new DynamoDBList();
        foreach (var encounter in encounters)
            list.Add(Inner.ToEntry(encounter));
        return list;
    }

    public object FromEntry(DynamoDBEntry entry)
    {
        var list = (DynamoDBList)entry;
        return list.Entries
            .Select(e => (Encounter)Inner.FromEntry(e))
            .ToList();
    }
}
