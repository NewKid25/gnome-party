using System;
using System.Collections.Generic;
using System.Text;

namespace Models;

public enum CharacterActionTargetRule
{
    Enemy = 0,
    Ally = 1,
    AllyOrSelf = 2,
    NoTargets = 3
}
