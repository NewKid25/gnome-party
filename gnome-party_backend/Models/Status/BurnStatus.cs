using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Status
{
    public sealed class BurnStatus : StatusEffect
    {
        public BurnStatus() { } // Parameterless constructor for deserialization or manual property setting

        // Constructor for creating a new BurnStatus instance with specified source, target, duration, and tick damage
        public BurnStatus(Character source, Character target, int duration = 3, int tickDamage = 2)
        {
            SourceCharacterId = source.Id;
            StatusOwnerCharacterId = target.Id;
            Duration = duration;
            DurationUnit = DurationUnit.TurnStart;
            AffectedCharacterIds = new List<string> { target.Id };
            ModifierValues = new Dictionary<string, double>
            {
                [StatusModifierKeys.TickDamage] = tickDamage
            };
        }

        // Creates a deep copy of the BurnStatus instance
        public override StatusEffect DeepCopy()
        {
            return new BurnStatus
            {
                SourceCharacterId = SourceCharacterId,
                StatusOwnerCharacterId = StatusOwnerCharacterId,
                Duration = Duration,
                DurationUnit = DurationUnit,
                AffectedCharacterIds = new List<string>(AffectedCharacterIds),
                ModifierValues = new Dictionary<string, double>(ModifierValues),
                StatusDescription = new Dictionary<string, string>(StatusDescription)
            };
        }

        // Method for applying the burn damage at the start of the target's turn
        public override void Process(Character character, List<CombatEvent> events)
        {
            int tickDamage = (int)ModifierValues.GetValueOrDefault(StatusModifierKeys.TickDamage, 0); // Get the tick damage from the modifier values, defaulting to 0 if not set
            if (tickDamage <= 0)
            {
                return;
            }
            character.Health -= tickDamage; // Apply the burn damage to the character's health
            events.Add(new CombatEvent("BurnTick", new StatusTickEventParams // Add a combat event to indicate that the burn damage has been applied
            {
                SourceId = SourceCharacterId,
                StatusAmount = tickDamage,
                CharacterId = character.Id,
            }));
        }
    }
}
