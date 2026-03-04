namespace GnomeParty.Models;

public class Campaign
{
    public List<Character> PlayerCharacters { get; set; }
    public List<CombatEncounter> Encounters { get; set; }

    public Campaign()
    {
        PlayerCharacters = new List<Character>();
        Encounters = [new CombatEncounter()];
    }
}
