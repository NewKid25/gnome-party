using GnomeParty.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models;

public class CombatEncounterGameState
{
    public List<Character> PlayerCharacters { get; set; }
    public List<Character> EnemyCharacters { get; set; }

    public CombatEncounterGameState() : this([], []) { }

    public CombatEncounterGameState(List<Character> playerCharacters, List<Character> enemyCharacters)
    {
        PlayerCharacters = playerCharacters;
        EnemyCharacters = enemyCharacters;
    }

    public CombatEncounterGameState DeepCopy()
    {
        var copy = new CombatEncounterGameState
        {
            PlayerCharacters = new List<Character>(PlayerCharacters.Select(pc => pc.DeepCopy())),
            EnemyCharacters = new List<Character>(EnemyCharacters.Select(ec => ec.DeepCopy()))
        };
        return copy;
    }

}
