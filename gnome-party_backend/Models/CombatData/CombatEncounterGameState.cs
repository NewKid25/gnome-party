using Models.CharacterData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models.CombatData;

public class CombatEncounterGameState
{
    public CombatEncounterGameState() : this([], []) { }
    public CombatEncounterGameState(List<Character> playerCharacters, List<Character> enemyCharacters)
    {
        PlayerCharacters = playerCharacters;
        EnemyCharacters = enemyCharacters;
    }
    public List<Character> EnemyCharacters { get; set; }
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
