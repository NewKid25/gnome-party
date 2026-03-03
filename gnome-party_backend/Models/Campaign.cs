namespace GnomeParty.Models;

public class Campaign
{
    public List<Character> PlayerCharacters { get; set; }
    public List<Encounter> Encounters { get; set; }

    public Campaign()
    {
        PlayerCharacters = new List<Character>();
        Encounters = new List<Encounter>();
    }
}
