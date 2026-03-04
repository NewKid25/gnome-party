using Amazon.DynamoDBv2.DataModel;
using GnomeParty.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GnomeParty.Models;

[DynamoDBTable("ActiveEncounterTable")]
public class ActiveCombatEncounter
{
    [DynamoDBHashKey]
    public string EncounterId { get; set; }
    public List<Character> PlayerCharacters { get; set; }
    public List<Character> EnemyCharacters { get; set; }
    public bool[] PlayerReadied { get; set; }
    public CombatRequest[] CombatRequests { get; set; }

    public ActiveCombatEncounter() : this([], []) { }

    public ActiveCombatEncounter(List<Character> playerCharacters, List<Character> enemyCharacters)
    {
        EncounterId = Guid.NewGuid().ToString();
        PlayerCharacters = playerCharacters;
        EnemyCharacters = enemyCharacters;
        PlayerReadied = new bool[PlayerCharacters.Count]; //should deafualt to all false
        CombatRequests = new CombatRequest[PlayerCharacters.Count];
    }
}
