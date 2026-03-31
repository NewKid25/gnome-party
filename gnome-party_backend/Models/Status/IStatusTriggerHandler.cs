using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Status
{
    public interface IStatusTriggerHandler
    {
        void Process(StatusEffect status, Character actingCharacter, List<CombatEvent> events);
    }
}
