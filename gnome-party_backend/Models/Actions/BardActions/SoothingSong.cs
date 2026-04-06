using Models.ActionMetaData;
using Models.CharacterData;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;

namespace Models.Actions.BardActions
{
    public sealed class SoothingSong : CharacterAction
    {
        public SoothingSong() : base("Soothing Song") // Pass the name of the action to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Soothing Song", "Heal an ally"); // Set the action description
        }
        public override AttackResolution ResolveAttack(Character user, Character target, CombatEncounterGameState gameState, bool isRedirected = false, bool isUnblockable = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user)); // Validate the user, target, and gameState parameters
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));
            var resolution = new AttackResolution(); // Create a new AttackResolution object to store the results of the attack
            resolution.HealInstances.Add(new HealInstance
            { 
                ActionName = AttackName,
                BaseHealing = 8,
                FinalHealing = 8,
                SourceCharacterId = user.Id,
                TargetCharacterId = target.Id,
            });
            return resolution;
        }
    }
}