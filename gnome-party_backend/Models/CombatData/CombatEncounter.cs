using Models;
using Models.CharacterData;

namespace Models.CombatData;

public class CombatEncounter : Encounter
{
    public List<Character> Enemies { get; set; }


    public CombatEncounter()
    {
        var skeleton_weak = new Character
        {
            Id = "test-enemy-1",
            Name = "skeleton_weak",
            MaxHealth = 10,
            Health = 10,
            ActionsDescriptions = [new CharacterActionDescription("punch", "A weak punch")]
        };
        var skeleton_strong = new Character
        {
            Id = "test-enemy-2",
            Name = "skeleton_strong",
            MaxHealth = 30,
            Health = 30,
            ActionsDescriptions = [new CharacterActionDescription("punch", "A weak punch")]
        };
        Enemies = [skeleton_weak, skeleton_strong];
    }

    public CombatEncounter(List<Character> _enemies)
    {
        Enemies = _enemies;
    }   
}
