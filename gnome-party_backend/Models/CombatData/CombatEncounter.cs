using Models;
using Models.CharacterData;

namespace Models.CombatData;

public class CombatEncounter : Encounter
{
    public List<Character> Enemies { get; set; }


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
}
