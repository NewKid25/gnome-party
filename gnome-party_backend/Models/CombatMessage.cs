using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class CombatMessage
    {
        public string Event { get; set; }
        public List<string> Params { get; set; }
        public CombatMessage(string events, List<string> parameters)
        {
            Event = events;
            Params = parameters;
        }
    }
}
