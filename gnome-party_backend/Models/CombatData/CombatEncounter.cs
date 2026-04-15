using Amazon.DynamoDBv2.DataModel;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.EncounterData;

namespace Models.CombatData;

public class CombatEncounter : Encounter
{
    public CombatEncounter()
    {
        var skeleton_weak = new Skeleton()
        {
            //half normal health
            Health = 10,
            MaxHealth = 10,
        };
        var skeleton_strong = new Skeleton();
        Enemies = [skeleton_weak, skeleton_strong];
    }
    public CombatEncounter(List<Character> _enemies)
    {
        Enemies = _enemies;
    }
    public List<Character> Enemies { get; set; }
}
