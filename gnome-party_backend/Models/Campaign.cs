using System;
using System.Collections.Generic;
using System.Text;

namespace Models;

class Campaign
{
    Character[] playerCharacters { get; set; }
    Encounter[] encounters { get; set; }
}
