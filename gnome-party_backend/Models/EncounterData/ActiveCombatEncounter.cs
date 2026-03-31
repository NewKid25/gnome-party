using Amazon.DynamoDBv2.DataModel;
using Models.CharacterData;
using Models.CombatData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models.EncounterData;

[DynamoDBTable("ActiveEncounterTable")]
public class ActiveCombatEncounter
{
    [DynamoDBHashKey]
    public string EncounterId { get; set; }
    public ActiveCombatEncounter() : this([], []) { }
    public ActiveCombatEncounter(List<Character> playerCharacters, List<Character> enemyCharacters)
    {
        EncounterId = Guid.NewGuid().ToString();
        GameState = new CombatEncounterGameState(playerCharacters, enemyCharacters);
        PlayerReadied = new List<bool>();
        playerCharacters.ForEach(_ => PlayerReadied.Add(false));
        CombatRequests = new List<CombatRequest>();
        playerCharacters.ForEach(_=> CombatRequests.Add(null));
    }
    public CombatEncounterGameState GameState { get; set; }
    public List<bool> PlayerReadied { get; set; }
    public List<CombatRequest> CombatRequests { get; set; }
}
