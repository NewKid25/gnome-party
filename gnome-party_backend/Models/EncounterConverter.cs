using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Models.CombatData;
using Models.EncounterData;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Models;

public class EncounterConverter : IPropertyConverter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        // Embeds "$type" field so polymorphic types survive round-trips
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = false
    };

    public DynamoDBEntry ToEntry(object value)
    {
        if (value is not Encounter encounter)
            throw new ArgumentException("Expected an Encounter");

        string json = JsonSerializer.Serialize(encounter, typeof(Encounter), Options);
        return new Primitive(json);
    }

    public object FromEntry(DynamoDBEntry entry)
    {
        string json = entry.AsString();
        var encounter = JsonSerializer.Deserialize<Encounter>(json, Options)
               ?? throw new InvalidOperationException("Failed to deserialize Encounter");
        if (encounter is CombatEncounter)
        {
            return encounter as CombatEncounter;
        }
        else
        {
            return encounter;
        }
    }
}
