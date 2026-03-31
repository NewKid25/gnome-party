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
                SourceId = status.SourceCharacterId,
                StatusAmount = tickDamage
                StatusType = status.StatusType,
                TargetId = character.Id,
                TargetName = character.Name,
            }));
        }
    }
}
