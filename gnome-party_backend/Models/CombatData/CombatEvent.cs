using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Models.CombatData
{
    public class CombatEvent
    {
        [JsonPropertyName("event")]
        public string Event { get; set; }
        [JsonPropertyName("params")]
        public object Params { get; set; }
        public CombatEvent(string evt, object parameters)
        {
            Event = evt;
            Params = parameters;
        }
    }
}
