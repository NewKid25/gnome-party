using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;
using Models.CombatData;

namespace Models.ActionMetaData
{
    public static class TargetingService
    {
        public static List<Character> GetTargetsTeam(CombatEncounterGameState gameState, string characterId)
        {
            if (gameState == null)
            {
                throw new ArgumentNullException(nameof(gameState));
            }
            if (characterId == null)
            {
                throw new ArgumentNullException(nameof(characterId));
            }
            if (gameState.PlayerCharacters.Any(c => c.Id == characterId))
            {
                return gameState.PlayerCharacters;
            }
            if (gameState.EnemyCharacters.Any(c => c.Id == characterId))
            {
                return gameState.EnemyCharacters;
            }
            throw new InvalidOperationException("Character does not exist in the game state.");
        }
        public static List<Character> GetOpposingTeam(CombatEncounterGameState gameState, string characterId)
        {
            if (gameState == null)
            {
                throw new ArgumentNullException(nameof(gameState));
            }
            if (characterId == null)
            {
                throw new ArgumentNullException(nameof(characterId));
            }
            if (gameState.PlayerCharacters.Any(c => c.Id == characterId))
            {
                return gameState.EnemyCharacters;
            }
            if (gameState.EnemyCharacters.Any(c => c.Id == characterId))
            {
                return gameState.PlayerCharacters;
            }
            throw new InvalidOperationException("Character does not exist in the game state.");
        }
        public static int FindTargetIndex(List<Character> team, string targetId)
        {
            if (team == null)
            {
                throw new ArgumentNullException(nameof(team));
            }
            if (targetId == null)
            {
                throw new ArgumentNullException(nameof(targetId));
            }
            int index = team.FindIndex(c => c.Id == targetId);
            if (index < 0)
            {
                throw new InvalidOperationException("Target does not exist in the provided team.");
            }
            return index;
        }
        public static List<Character> GetTargetAndAdjacentAllies(CombatEncounterGameState gameState, string targetId)
        {
            var team = GetTargetsTeam(gameState, targetId);
            int targetIndex = FindTargetIndex(team, targetId);
            var targets = new List<Character>();
            if (targetIndex - 1 >= 0)
            {
                targets.Add(team[targetIndex - 1]);
            }
            targets.Add(team[targetIndex]);
            if (targetIndex + 1 < team.Count)
            {
                targets.Add(team[targetIndex + 1]);
            }
            return targets;
        }
        public static List<Character> GetAdjacentAlliesOnly(CombatEncounterGameState gameState, string targetId)
        {
            var team = GetTargetsTeam(gameState, targetId);
            int targetIndex = FindTargetIndex(team, targetId);
            var targets = new List<Character>();
            if (targetIndex - 1 >= 0)
            {
                targets.Add(team[targetIndex - 1]);
            }
            if (targetIndex + 1 < team.Count)
            {
                targets.Add(team[targetIndex + 1]);
            }
            return targets;
        }
        public static List<Character> GetAllCharacters(CombatEncounterGameState gameState)
        {
            if (gameState == null)
            {
                throw new ArgumentNullException(nameof(gameState));
            }
            var all = new List<Character>();
            foreach (var player in gameState.PlayerCharacters)
            {
                all.Add(player);
            }
            foreach (var enemy in gameState.EnemyCharacters)
            {
                all.Add(enemy);
            }
            return all;
        }
    }
}
