using System;
using System.Collections.Generic;
using System.Text;

namespace Models.CombatData;

public class CombatResult
{
    public CombatRequest Request { get; set; }
    public CombatEncounterGameState GameState { get; set; }
    public List<CombatEvent> Events { get; set; }

    public CombatResult(CombatRequest request, CombatEncounterGameState gameState, List<CombatEvent> events)
    {
        Request = request;
        GameState = gameState;
        Events = events;
    }
}
