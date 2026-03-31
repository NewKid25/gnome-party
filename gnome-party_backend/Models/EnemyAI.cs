using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;

namespace Models
{
    public static class EnemyAI
    {
        public static Character SelectTarget(List<Character> playerCharacters)
        {
            if (playerCharacters == null || playerCharacters.Count == 0)
            {
                throw new ArgumentException("Player characters list cannot be null or empty.");
            }
            var alivePlayers = playerCharacters.Where(p => p.Health > 0).ToList();
            if (alivePlayers.Count == 0)
            {
                throw new InvalidOperationException("No alive player characters to target.");
            }
            return alivePlayers.OrderBy(p => (double)p.Health / Math.Max(1, p.MaxHealth)).ThenBy(p => p.Health).ThenBy(p => p.Id).First();
        }
        public static AttackContext CreateEnemyAttackContext(Character enemy, CharacterAction action, List<Character> playerCharacters)
        {
            var target = SelectTarget(playerCharacters);
            return new AttackContext(enemy, action, target);
        }
    }
}
