namespace Models;

public class CombatEncounter
{
    List<Character> Enemies { get; set; }


    public CombatEncounter()
    {
        Enemies = new List<Character>();
    }

    public CombatEncounter(List<Character> _enemies)
    {
        Enemies = _enemies;
    }   
}
