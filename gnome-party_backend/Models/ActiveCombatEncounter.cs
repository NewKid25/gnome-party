using Amazon.DynamoDBv2.DataModel;
using Models.CharacterData;
using Models.CombatData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models;

[DynamoDBTable("ActiveEncounterTable")]
public class ActiveCombatEncounter
{
    [DynamoDBHashKey]
    public string EncounterId { get; set; }
    public CombatEncounterGameState GameState { get; set; }
    public List<bool> PlayerReadied { get; set; }
    public List<CombatRequest> CombatRequests { get; set; }

    public ActiveCombatEncounter() : this([], []) { }

    public ActiveCombatEncounter(List<Character> playerCharacters, List<Character> enemyCharacters)
    {
        EncounterId = Guid.NewGuid().ToString();
        GameState = new CombatEncounterGameState(playerCharacters, enemyCharacters);
        PlayerReadied = new List<bool>();
        PlayerReadied.ForEach(_ => PlayerReadied.Add(false)); //initialize all players as not readied
        //PlayerReadied = [playerCharacters.Count]; //should deafualt to all false
        CombatRequests = new List<CombatRequest>();
    }
}
