using Amazon.DynamoDBv2.DataModel;
using Models.CharacterData;

namespace Models.CombatData;

public class CombatEncounterGameState
{
    public CombatEncounterGameState() : this([], []) { }
    public CombatEncounterGameState(List<Character> playerCharacters, List<Character> enemyCharacters)
    {
        PlayerCharacters = playerCharacters;
        EnemyCharacters = enemyCharacters;
    }

    [DynamoDBProperty(typeof(CharacterListConverter))]
    public List<Character> EnemyCharacters { get; set; }

    [DynamoDBProperty(typeof(CharacterListConverter))]
    public List<Character> PlayerCharacters { get; set; }

    public CombatEncounterGameState DeepCopy()
    {
        var copy = new CombatEncounterGameState
        {
            EnemyCharacters = new List<Character>(EnemyCharacters.Select(ec => ec.DeepCopy())),
            PlayerCharacters = new List<Character>(PlayerCharacters.Select(pc => pc.DeepCopy())),
        };
        return copy;
    }
}
