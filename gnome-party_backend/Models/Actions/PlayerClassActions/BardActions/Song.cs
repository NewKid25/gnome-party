using Models.ActionMetaData;
using Models.CharacterData;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using static Models.CharacterData.PlayerCharacterClasses.Bard;

namespace Models.Actions.PlayerClassActions.BardActions
{
    // Song: Cycle through a series of song based subset attacks
    public sealed class Song : CharacterAction
    {
        public Song() : base("Song") // Pass the name of the action to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Song", "Cycle through a playlist of Song attacks"); // Set the action description
        }
        public override AttackResolution ResolveAttack(Character user, Character target, CombatEncounterGameState gameState, bool isRedirected = false, bool isUnblockable = false)
        {
            // Null check user, target, and gameState
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if(gameState == null) throw new ArgumentNullException(nameof(gameState));

            if(user is not Bard bard) { throw new InvalidOperationException("Song can only be used by a Bard"); } // Confirm if the user is a bard

            CharacterAction chosenAction; // Variable to hold the chosen action

            string currentSong = bard.GetCurrentSong(); // Retrieve the bard's current song
            
            // Choose the correct action based on the current song
            switch(currentSong)
            {
                case BardSongs.Soothing:
                    chosenAction = new SoothingSong();
                    break;
                case BardSongs.Inspiring:
                    chosenAction = new InspiringSong();
                    break;
                case BardSongs.Frightening:
                    chosenAction = new FrighteningSong();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown bardic song: {currentSong}");
            }

            var resolution = chosenAction.ResolveAttack (user, target, gameState, isRedirected, isUnblockable); // Return the chosen song's action 

            bard.ChangeBardicSong(currentSong); // change the bard's current song

            return resolution; // return the attack resolution
        }
        public override List<Character> ReturnEligibleTargets(Character user, CombatEncounterGameState gameState)
        {
            // Null check user and gameState
            if(user == null) throw new ArgumentNullException(nameof(user));
            if(gameState == null) throw new ArgumentNullException(nameof(gameState));

            if (user is not Bard bard) { throw new InvalidOperationException("Song can only be performed by a Bard."); } // Check if the user is a Bard

            CharacterAction chosenAction; // Variable to hold the chosen action

            string currentSong = bard.GetCurrentSong(); // Retrieve the bard's current song

            // Choose the correct action based on the current song
            switch (currentSong)
            {
                case BardSongs.Soothing:
                    chosenAction = new SoothingSong();
                    break;
                case BardSongs.Inspiring:
                    chosenAction = new InspiringSong();
                    break;
                case BardSongs.Frightening:
                    chosenAction = new FrighteningSong();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown bardic song: {currentSong}");
            }
            return chosenAction.ReturnEligibleTargets(user, gameState); // Return the chosen song's eligible targets
        }
    }
}
