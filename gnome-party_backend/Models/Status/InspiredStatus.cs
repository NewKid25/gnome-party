using Models.CharacterData;

namespace Models.Status
{
    public sealed class InspiredStatus : StatusEffect
    {
        public InspiredStatus() { }

        public InspiredStatus(Character source, Character ally)
        {
            SourceCharacterId = source.Id;
            StatusOwnerCharacterId = ally.Id;
            Duration = 1;
            DurationUnit = DurationUnit.TurnEnd;

            ModifierValues = new Dictionary<string, double>
            {
                [StatusModifierKeys.OutgoingDamageMultiplier] = 1.5 // Boost damage by 50% (150 % attack power)
            };

            AffectedCharacterIds = new List<string> { ally.Id };

            StatusDescription = new Dictionary<string, string>
            {
                ["AppliedText"] = $"{source.Name} inspires {ally.Name}.",
                ["ActiveText"] = $"{ally.Name}'s damage is increased.",
                ["ExpiredText"] = $"{ally.Name} is no longer inspired."
            };
        }

        public override StatusEffect DeepCopy()
        {
            return new InspiredStatus
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

        public override double ModifyOutgoingDamageMultiplier(
            Character source,
            Character target,
            double currentMultiplier,
            bool isUnblockable)
        {
            if (source.Id != StatusOwnerCharacterId)
            {
                return currentMultiplier;
            }

            if (ModifierValues.TryGetValue(StatusModifierKeys.OutgoingDamageMultiplier, out var value))
            {
                return currentMultiplier * value;
            }

            return currentMultiplier;
        }
    }
}
