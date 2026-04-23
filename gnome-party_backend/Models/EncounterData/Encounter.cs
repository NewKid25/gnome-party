using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Models.EncounterData;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Models.CombatData.CombatEncounter), typeDiscriminator: "CombatEncounter")]
public class Encounter
{
    public int ActionTime { get; set; } = 10;
    public Encounter(){}
}
