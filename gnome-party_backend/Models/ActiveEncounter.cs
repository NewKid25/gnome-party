using Amazon.DynamoDBv2.DataModel;
using GnomeParty.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models;

[DynamoDBTable("ActiveEncounterTable")]
public class ActiveCombatEncounter
{
    [DynamoDBHashKey]
    public string EncounterId { get; set; }
    public List<Character> PlayerCharacters { get; set; }
    public List<Character> EnemyCharacters { get; set; }
    public List<bool> PlayerReadied { get; set; } = [];

    public ActiveCombatEncounter() : this([], []) { }

    public ActiveCombatEncounter(List<Character> playerCharacters, List<Character> enemyCharacters)
    {
        EncounterId = Guid.NewGuid().ToString();
        PlayerCharacters = playerCharacters;
        EnemyCharacters = enemyCharacters;
        foreach (var _ in PlayerCharacters)
        {
            PlayerReadied.Add(false);
        }
    }
}
