using Amazon.DynamoDBv2.DataModel;
using GnomeParty.Models;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GnomeParty.Models;

[DynamoDBTable("ActiveEncounterTable")]
public class ActiveCombatEncounter
{
    [DynamoDBHashKey]
    public string EncounterId { get; set; }
    public CombatEncounterGameState GameState { get; set; }
    public bool[] PlayerReadied { get; set; }
    public CombatRequest[] CombatRequests { get; set; }

    public ActiveCombatEncounter() : this([], []) { }

    public ActiveCombatEncounter(List<Character> playerCharacters, List<Character> enemyCharacters)
    {
        EncounterId = Guid.NewGuid().ToString();
        GameState = new CombatEncounterGameState(playerCharacters, enemyCharacters);
        PlayerReadied = new bool[playerCharacters.Count]; //should deafualt to all false
        CombatRequests = new CombatRequest[playerCharacters.Count];
    }
}
