using Models.CharacterData;
using Models.CombatData;

namespace Models.Status
{
    // Enum to represent the unit of time for status effect duration
    public enum DurationUnit 
    {
        Campaign,
        Encounter,
        TurnStart,
        TurnEnd
    }

    // Static class to hold constant keys for status effect modifiers
    public static class StatusModifierKeys
    {
        public const string DamageReduction = "DamageReduction";
        public const string IncomingDamageMultiplier = "IncomingDamageMultiplier";
        public const string OutgoingDamageMultiplier = "OutgoingDamageMultiplier";
        public const string TickDamage = "TickDamage";
    }
    public class StatusEffect
    {
        public List<string> AffectedCharacterIds { get; set; } = new();
        public int Duration { get; set; }
        public DurationUnit DurationUnit { get; set; }
        public Dictionary<string, double> ModifierValues { get; set; } = new();
        public Dictionary<string, string> StatusDescription { get; set; } = new();
        public string StatusOwnerCharacterId { get; set; } = string.Empty;
        public string SourceCharacterId { get; set; } = string.Empty;
        
        // Provides a method to create a deep copy of the StatusEffect instance
        public virtual StatusEffect DeepCopy()
        {
            return new StatusEffect 
            {
                AffectedCharacterIds = new List<string>(AffectedCharacterIds), 
                Duration = Duration, 
                DurationUnit = DurationUnit,
                ModifierValues = new Dictionary<string, double>(ModifierValues), 
                StatusDescription = new Dictionary<string, string>(StatusDescription),
                StatusOwnerCharacterId = StatusOwnerCharacterId, 
                SourceCharacterId = SourceCharacterId, 
            };
        }

        // Provides a method to process the status effect
        public virtual void Process(Character actingCharacter, List<CombatEvent> events){}

        // Provides a method to modify incoming damage multiplier
        public virtual double ModifyIncomingDamageMultiplier(
            Character source,
            Character target,
            double currentMultiplier,
            bool isUnblockable) { return currentMultiplier; }

        // Provides a method to modify outgoing damage multiplier
        public virtual double ModifyOutgoingDamageMultiplier(
            Character source,
            Character target,
            double currentMultiplier,
            bool isUnblockable)   { return currentMultiplier; }

        // Provides a method to modify damage reduction
        public virtual double ModifyDamageReduction(
            Character source,
            Character target,
            double currentReduction,
            bool isUnblockable)   { return currentReduction; }

        // Provides a method to modify the redirect target of an attack
        public virtual Character ModifyRedirectTarget(
            Character attacker,
            Character originalTarget,
            Character currentTarget,
            CombatEncounterGameState gameState,
            bool isUnblockable, bool isUnRedirectable)  { return currentTarget; }
    }
}