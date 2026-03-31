using Models.CharacterData;
using Models.CombatData;

namespace Models.GameMetaData;

public class Campaign
{
    public List<CombatEncounter> Encounters { get; set; }
    public List<Character> PlayerCharacters { get; set; }
    public Campaign()
    {
        PlayerCharacters = new List<Character>();
        Encounters = [new CombatEncounter()];
    }
}
