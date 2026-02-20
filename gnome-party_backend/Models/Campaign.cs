using System;
using System.Collections.Generic;
using System.Text;

namespace Models;

public class Campaign
{
    List<Character> playerCharacters { get; set; }
    List<Encounter> encounters { get; set; }

    public Campaign()
    {
        playerCharacters = new List<Character>();
        encounters = new List<Encounter>();
    }
}
