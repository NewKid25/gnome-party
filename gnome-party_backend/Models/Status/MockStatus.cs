using Models.CharacterData;
using Models.CombatData;

namespace Models.Status
{
    public sealed class MockStatus : StatusEffect
    {
        public MockStatus() { }
        public MockStatus(Character user, Character enemy)
        {
            Duration = 2; // Last until the start of the user's next turn
            DurationUnit = DurationUnit.TurnEnd; // Mock is active until the end of the user's current turn
            SourceCharacterId = user.Id; // The character who applied the mockery
            AffectedCharacterIds = new List<string> { enemy.Id }; // The character whose targeting will change to the source character
            StatusOwnerCharacterId = user.Id; // The mock status is owned by the user who applied it
            StatusDescription = new Dictionary<string, string> // Text descriptions for the mock status
            {
                ["AppliedText"] = $"{user.Name} starts mocking {enemy.Name}.",
                ["ActiveText"] = $"{enemy.Name} targets {user.Name}",
                ["ExpiredText"] = $"{user.Name} is no longer mocking {enemy.Name}."
            };
        }
        public override StatusEffect DeepCopy() // Creates a deep copy of the MockStatus instance
        {
            return new MockStatus
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
        public override Character ModifyRedirectTarget(Character attacker, Character originalTarget, Character currentTarget, CombatEncounterGameState gameState, bool isUnblockable)
        {
            // Null checking with error handling
            if(attacker == null) throw new ArgumentNullException(nameof(attacker));
            if(originalTarget == null) throw new ArgumentNullException(nameof(originalTarget));
            if(currentTarget == null) throw new ArgumentNullException(nameof(currentTarget));
            if(gameState == null) throw new ArgumentNullException(nameof(gameState));

            if (!AffectedCharacterIds.Contains(attacker.Id)) { return currentTarget; } // If the attacker is not affected by this mock, return the current target unchanged
            
            // Find the mocker character who applied this mock status
            var mocker = gameState.PlayerCharacters.Concat(gameState.EnemyCharacters).FirstOrDefault(c => c.Id == StatusOwnerCharacterId && c.Health > 0);
            if(mocker == null) { return currentTarget; }
            return mocker; // Return mocker as the new target
        }
    }
}
