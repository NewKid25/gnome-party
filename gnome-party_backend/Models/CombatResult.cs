using GnomeParty.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models;

public class CombatResult
{
    public CombatRequest Request { get; set; }
    public CombatEncounterGameState GameState { get; set; }

    public CombatResult(CombatRequest request, CombatEncounterGameState gameState)
    {
        Request = request;
        GameState = gameState;
    }
}
