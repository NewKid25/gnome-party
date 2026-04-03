using Models.CharacterData;
using Models.CombatData;

namespace Models.Status
{
    public enum DurationUnit
    {
        Campaign,
        Encounter,
        TurnStart,
        TurnEnd
    }
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
        public virtual void Process(Character actingCharacter, List<CombatEvent> events){}
        public virtual double ModifyIncomingDamageMultiplier(
            Character source,
            Character target,
            double currentMultiplier,
            bool isUnblockable) { return currentMultiplier; }
        public virtual double ModifyOutgoingDamageMultiplier(
            Character source,
            Character target,
            double currentMultiplier,
            bool isUnblockable)   {    return currentMultiplier; }
        public virtual double ModifyDamageReduction(
            Character source,
            Character target,
            double currentReduction,
            bool isUnblockable)   {     return currentReduction;  }
        public virtual Character ModifyRedirectTarget(
            Character source,
            Character originalTarget,
            Character currentTarget,
            CombatEncounterGameState gameState,
            bool isUnblockable)  {   return currentTarget; }
    }
}