using Models.CharacterData;

namespace Models.Status
{
    public sealed class ParryStatus : StatusEffect
    {
        public ParryStatus() { }
        public ParryStatus(Character user, Character enemy)
        {
            AffectedCharacterIds = new List<string> { enemy.Id };
            Duration = 1;
            DurationUnit = DurationUnit.TurnStart;
            SourceCharacterId = user.Id;
            StatusDescription = new Dictionary<string, string>
            {
                ["AppliedText"] = $"{user.Name} activates parry against {enemy.Name}.",
                ["ActiveText"] = $"{enemy.Name}'s attack is parried by {user.Name}",
                ["ExpiredText"] = $"{user.Name} is no longer parrying {enemy.Name}."
            };
            StatusOwnerCharacterId = user.Id;
            StatusType = StatusTypes.Parry;
        }
        public override StatusEffect DeepCopy()
        {
            return new ParryStatus
            {
                AffectedCharacterIds = new List<string>(AffectedCharacterIds),
                Duration = Duration,
                DurationUnit = DurationUnit,
                ModifierValues = new Dictionary<string, double>(ModifierValues),
                SourceCharacterId = SourceCharacterId,
                StatusDescription = new Dictionary<string, string>(StatusDescription),
                StatusId = StatusId,
                StatusOwnerCharacterId = StatusOwnerCharacterId,
                StatusType = StatusType,
            };
        }
    }
}
