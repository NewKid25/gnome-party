using Models.ActionMetaData;
using Models.CharacterData;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;

namespace Models.Actions.BardActions
{
    public sealed class Song : CharacterAction
    {
        public Song() : base("Song") // Pass the name of the action to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Song", "Cycle through a playlist of Song attacks"); // Set the action description
        }
        public override AttackResolution ResolveAttack(Character user, Character target, CombatEncounterGameState gameState, bool isRedirected = false, bool isUnblockable = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (user == null) throw new ArgumentNullException(nameof(target));
            if (user is Bard bard) // Verify that the current user is a bard
            {
                var resolution = new AttackResolution();
                resolution.AttackInstances.Add(new AttackInstance 
                { 
                    ActionName = AttackName,
                    SourceCharacterId = user.Id,
                });
                return resolution;
            }
            throw new InvalidOperationException("Song can only be performed by a Bard.");
        }
    }
}
