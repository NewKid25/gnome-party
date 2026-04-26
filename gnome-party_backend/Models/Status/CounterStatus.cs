using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Status
{
    public sealed class CounterStatus : StatusEffect
    {
        public CounterStatus() { }
        public CounterStatus(Character user, Character enemy)
        {
            Duration = 1; // Lasts until the start of the user's next turn
            DurationUnit = DurationUnit.TurnStart; // Counter is active until the start of the user's next turn
            SourceCharacterId = user.Id; // The character who applied the Counter
            AffectedCharacterIds = new List<string> { enemy.Id }; // The character whose attack is being parried
            StatusDescription = new Dictionary<string, string> // Text descriptions for the parry status
            {
                ["AppliedText"] = $"{user.Name} assumes a defensive stance {enemy.Name}.",
                ["ActiveText"] = $"{enemy.Name}'s attack is countered by {user.Name}",
                ["ExpiredText"] = $"{user.Name} leaves their defensive stance {enemy.Name}."
            };
            StatusOwnerCharacterId = user.Id; // The parry status is owned by the user who applied it
        }
        public override StatusEffect DeepCopy() // Creates a deep copy of the ParryStatus instance
        {
            return new CounterStatus
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
        public override double ModifyIncomingDamageMultiplier(Character source, Character target, double currentMultiplier, bool isUnblockable)
        {
            if (isUnblockable) // If the attack is unblockable, the parry does not reduce damage
            {
                return currentMultiplier;
            }
            if (target.Id == StatusOwnerCharacterId && AffectedCharacterIds.Contains(source.Id)) // If the target is the owner of the parry and the source is the character being parried, reduce damage to 0
            {
                return 0;
            }
            return currentMultiplier;
        }
        public override IEnumerable<AttackInstance> CreateCounterAttacks(
            Character source,
            Character target,
            AttackInstance incomingAttack,
            CombatEncounterGameState gameState)
        {
            int counterDamage = 6;
            if (incomingAttack.IsUnblockable)
            {
                return Enumerable.Empty<AttackInstance>();
            }

            if (target.Id != StatusOwnerCharacterId)
            {
                return Enumerable.Empty<AttackInstance>();
            }

            if (!AffectedCharacterIds.Contains(source.Id))
            {
                return Enumerable.Empty<AttackInstance>();
            }

            return new[]
            {
                new AttackInstance
                {
                    ActionName = "Counter Attack",
                    BaseDamage = counterDamage,
                    FinalDamage = counterDamage,
                    SourceCharacterId = target.Id,
                    TargetCharacterId = source.Id,
                    IsUnblockable = true,
                }
            };
        }
    }
}