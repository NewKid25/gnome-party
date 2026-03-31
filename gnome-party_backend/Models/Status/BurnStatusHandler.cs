using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Status
{
    public class BurnStatusHandler : IStatusTriggerHandler
    {
        public void Process(StatusEffect status, Character character, List<CombatEvent> events)
        {
            int tickDamage = (int)status.ModifierValues.GetValueOrDefault(StatusModifierKeys.TickDamage, 0);
            if(tickDamage <= 0)
            {
                return;
            }
            character.Health -= tickDamage;
            events.Add(new CombatEvent("BurnTick", new StatusTickEventParams
            {
                StatusType = status.StatusType,
                SourceId = status.SourceCharacterId,
                TargetId = character.Id,
                TargetName = character.Name,
                StatusAmount = tickDamage
            }));
        }
    }
}
