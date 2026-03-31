using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Status
{
    public enum DurationUnit
    {
        Encounter,
        TurnStart,
        TurnEnd,
        Campaign,
    }

    public static class StatusModifierKeys
    {
        public const string DamageReduction = "DamageReduction";
        public const string RedirectTarget = "RedirectTarget";
        public const string TickDamage = "TickDamage";
        public const string TickHeal = "TickHeal";
        public const string IncomingDamageMultiplier = "IncomingDamageMultiplier";
        public const string OutgoingDamageMultiplier = "OutgoingDamageMultiplier";
    }

    public static class StatusTypes
    {
        public const string Block = "Block";
        public const string Burn = "Burn";
    }

    public class StatusEffect
    {
        public string StatusId { get; set; } = Guid.NewGuid().ToString();
        public string StatusType { get; set; } = string.Empty;
        public string SourceCharacterId { get; set; } = string.Empty;
        public string StatusOwnerCharacterId { get; set; } = string.Empty;
        public int Duration { get; set; }
        public DurationUnit DurationUnit { get; set; }
        public List<string> AffectedCharacterIds { get; set; } = new();
        public Dictionary<string, double> ModifierValues { get; set; } = new();
        public Dictionary<string, string> StatusDescription { get; set; } = new();
        public virtual StatusEffect DeepCopy()
        {
            return new StatusEffect 
            {
                StatusId = StatusId, 
                StatusType = StatusType, 
                SourceCharacterId = SourceCharacterId, 
                StatusOwnerCharacterId = StatusOwnerCharacterId, 
                Duration = Duration, 
                DurationUnit = DurationUnit, 
                AffectedCharacterIds = new List<string>(AffectedCharacterIds), 
                ModifierValues = new Dictionary<string, double>(ModifierValues), 
                StatusDescription = new Dictionary<string, string>(StatusDescription) };
        }
    }
}