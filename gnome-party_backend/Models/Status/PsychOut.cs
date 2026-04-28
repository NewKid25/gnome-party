using Models.CharacterData;

namespace Models.Status
{
    // Increases the incoming damage of the affected character
    public sealed class PsychOut : StatusEffect
    {
        public PsychOut() { }
        public PsychOut(Character user, Character enemy, double psychoOutAmount)
        {
            SourceCharacterId = user.Id;
            StatusOwnerCharacterId = enemy.Id;
            Duration = 1;
            DurationUnit = DurationUnit.TurnStart;
            StatusDescription = new Dictionary<string, string> // Descriptions for different stages of the status
            {
                ["AppliedText"] = $"{enemy.Name} has been psyched out.",
                ["ActiveText"] = $"{enemy.Name} will receive more damage.",
                ["ExpiredText"] = $"{enemy.Name} is no longer psyched out."
            };
            StatusOwnerCharacterId = enemy.Id; // The character affected by the status
            AffectedCharacterIds = new List<string> { enemy.Id }; // The character whose attacks will be reduced
            ModifierValues = new Dictionary<string, double>
            {
               // Increase incoming damage by the specified multiplier (e.g., 1.25 for 25% more damage)
               { StatusModifierKeys.IncomingDamageMultiplier, psychoOutAmount }
            };
        }
        // Make a deep copy of the status effect
        public override StatusEffect DeepCopy()
        {
            return new WeakenedStatus
            {
                AffectedCharacterIds = new List<string>(AffectedCharacterIds),
                Duration = Duration,
                DurationUnit = DurationUnit,
                ModifierValues = new Dictionary<string, double>(ModifierValues),
                SourceCharacterId = SourceCharacterId,
                StatusDescription = new Dictionary<string, string>(StatusDescription),
                StatusOwnerCharacterId = StatusOwnerCharacterId,
            };
        }
        // Modify the incoming damage multiplier to reflect the effect of PsychOut Status
        public override double ModifyIncomingDamageMultiplier(Character source, Character target, double currentMultiplier, bool isUnblockable)
        {
            if (AffectedCharacterIds.Contains(source.Id)
                && ModifierValues.TryGetValue(StatusModifierKeys.IncomingDamageMultiplier, out double psychoOutAmount))
            {
                return currentMultiplier * psychoOutAmount;
            }
            return currentMultiplier;
        }
    }
}

