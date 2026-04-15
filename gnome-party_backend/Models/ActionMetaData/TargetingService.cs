using Models.CharacterData;
using Models.CombatData;

namespace Models.ActionMetaData
{
    // This service provides utility methods for determining valid targets for character actions based on the current game state and the specified target character
    public static class TargetingService
    {
        public static int FindTargetIndex(List<Character> team, string targetId)
        {
            if (team == null) // Null check for the team list
            {
                throw new ArgumentNullException(nameof(team));
            }
            if (targetId == null) // Null check for the target ID
            {
                throw new ArgumentNullException(nameof(targetId));
            }
            int index = team.FindIndex(c => c.Id == targetId); // Find the index of the target character in the team list
            if (index < 0) // If the target character is not found in the team list, throw an exception
            {
                throw new InvalidOperationException("Target does not exist in the provided team.");
            }
            return index; // Return the index of the target character in the team list
        }
        public static List<Character> GetAdjacentAlliesOnly(CombatEncounterGameState gameState, string targetId)
        {
            var team = GetTargetsTeam(gameState, targetId); // Get the team of the target character based on the current game state and the target ID
            int targetIndex = FindTargetIndex(team, targetId); // Find the index of the target character in the team list
            var targets = new List<Character>(); // Initialize a list to hold the adjacent allies
            if (targetIndex - 1 >= 0) // Check if there is an ally to the left of the target character and add it to the targets list if it exists
            {
                targets.Add(team[targetIndex - 1]);
            }
            if (targetIndex + 1 < team.Count) // Check if there is an ally to the right of the target character and add it to the targets list if it exists
            {
                targets.Add(team[targetIndex + 1]);
            }
            return targets; // Return the list of adjacent allies to the target character
        }
        public static List<Character> GetAllCharacters(CombatEncounterGameState gameState)
        {
            if (gameState == null) // Null check for the game state
            {
                throw new ArgumentNullException(nameof(gameState));
            }
            var all = new List<Character>(); // Initialize a list to hold all characters in the game state
            foreach (var player in gameState.PlayerCharacters) // Add all player characters to the list
            {
                all.Add(player);
            }
            foreach (var enemy in gameState.EnemyCharacters) // Add all enemy characters to the list
            {
                all.Add(enemy);
            }
            return all; // Return the list of all characters in the game state
        }
        public static List<Character> GetOpposingTeam(CombatEncounterGameState gameState, string characterId)
        {
            if (gameState == null) // Null check for the game state
            {
                throw new ArgumentNullException(nameof(gameState));
            }
            if (characterId == null) // Null check for the character ID
            {
                throw new ArgumentNullException(nameof(characterId));
            }
            if (gameState.PlayerCharacters.Any(c => c.Id == characterId)) // Check if the character belongs to the player's team and return the enemy team if it does
            {
                return gameState.EnemyCharacters;
            }
            if (gameState.EnemyCharacters.Any(c => c.Id == characterId)) // Check if the character belongs to the enemy team and return the player's team if it does
            {
                return gameState.PlayerCharacters;
            }
            throw new InvalidOperationException("Character does not exist in the game state.");
        }
        public static List<Character> GetTargetAndAdjacentAllies(CombatEncounterGameState gameState, string targetId)
        {
            var team = GetTargetsTeam(gameState, targetId); // Get the team of the target character based on the current game state and the target ID
            int targetIndex = FindTargetIndex(team, targetId); // Find the index of the target character in the team list
            var targets = new List<Character>(); // Initialize a list to hold the target character and its adjacent allies
            if (targetIndex - 1 >= 0) // Check if there is an ally to the left of the target character and add it to the targets list if it exists
            {
                targets.Add(team[targetIndex - 1]);
            }
            targets.Add(team[targetIndex]); // Add the target character itself to the targets list
            if (targetIndex + 1 < team.Count) // Check if there is an ally to the right of the target character and add it to the targets list if it exists
            {
                targets.Add(team[targetIndex + 1]);
            }
            return targets; // Return the list of the target character and its adjacent allies
        }
        public static List<Character> GetTargetsTeam(CombatEncounterGameState gameState, string characterId)
        {
            if (gameState == null) // Null check for the game state
            {
                throw new ArgumentNullException(nameof(gameState));
            }
            if (characterId == null) // Null check for the character ID
            {
                throw new ArgumentNullException(nameof(characterId));
            }
            if (gameState.PlayerCharacters.Any(c => c.Id == characterId)) // Check if the character belongs to the player's team and return the player's team if it does
            {
                return gameState.PlayerCharacters;
            }
            if (gameState.EnemyCharacters.Any(c => c.Id == characterId)) // Check if the character belongs to the enemy team and return the enemy team if it does
            {
                return gameState.EnemyCharacters;
            }
            throw new InvalidOperationException("Character does not exist in the game state.");
        }
    }
}
