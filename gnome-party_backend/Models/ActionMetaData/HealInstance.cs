
namespace Models.ActionMetaData
{
    public enum HealCalculationType
    {
        Flat,
        FromDamageDealt
    }

    // Data Transfer Object for Healing Actions
    public class HealInstance
    {
        public string SourceCharacterId { get; set; } = "";
        public string TargetCharacterId { get; set; } = "";
        public string ActionName { get; set; } = "";

        public int BaseHealing { get; set; }
        public int FinalHealing { get; set; }

        public HealCalculationType CalculationType { get; set; } = HealCalculationType.Flat; // default healing type
        public double HealingRatio { get; set; } = 1.0; // default healing ratio from damage dealt

        /*
         * Only count damage from attacks with this action name
         * Leave null/empty to count all attacks from the source in this resolution
         */
        public string? DamageSourceActionNameFilter { get; set; }

        /*
         * If true, only count damage done by the same source character
         */
        public bool RequireSameSourceCharacter { get; set; } = true;
    }
}
