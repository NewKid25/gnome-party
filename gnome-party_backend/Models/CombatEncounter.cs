namespace GnomeParty.Models;

public class CombatEncounter : Encounter
{
    public List<Character> Enemies { get; set; }


    public CombatEncounter()
    {
        var skeleton = new Character
        {
            Id = "test-enemy",
            Name = "skeleton",
            MaxHealth = 10,
            Health = 10,
            ActionsDescriptions = [new CharacterActionDescription("punch", "A weak punch")]
        };
        Enemies = [skeleton];
    }

    public CombatEncounter(List<Character> _enemies)
    {
        Enemies = _enemies;
    }   
}
