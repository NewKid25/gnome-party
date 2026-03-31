using System.Diagnostics;
using Models.CharacterData;
using Models.CombatData;

namespace Models.AI;

internal class SkeletonAI : CharacterAI
{
    /* public override CombatRequest ChooseAction(List<string> actions, List<Character> enemies, List<Character> allies)
     {
         var action = GetDefaultAction(actions);
         if (actions.Contains("Bone Slash"))
         {
             action = "Bone Slash";
         }
         var target = enemies[0]; 
         return new CombatRequest
         {
             Action = action,
             TargetCharacterId = target.Id
         };
     }*/
    public override CombatRequest ChooseAction(Character self, List<string> actions, List<Character> enemies, List<Character> allies)
    {
        if(actions == null || actions.Count == 0)
        {
            throw new ArgumentException("Actions list cannot be null or empty.");
        }
        if(enemies == null || enemies.Count == 0)
        {
            throw new ArgumentException("Enemies list cannot be null or empty.");
        }
        var aliveEnemies = enemies.Where(e => e.Health > 0).ToList();
        if(aliveEnemies.Count == 0)
        {
            throw new InvalidOperationException("No alive enemies to target.");
        }
        var action = ChooseActionName(self, actions, enemies, allies);
        var target = aliveEnemies.OrderBy(e => (double)e.Health / Math.Max(1, e.MaxHealth)).ThenBy(e => e.Health).ThenByDescending(e => e.MaxHealth).First();
        return new CombatRequest
        {
            Action = actions.Contains("Bone Slash") ? "Bone Slash" : GetDefaultAction(actions),
            TargetCharacterId = enemies[0].Id
        };
    }
    protected override string ChooseActionName(Character self, List<string> actions, List<Character> enemies, List<Character> allies)
    {
        bool hasBoneSlash = actions.Contains("Bone Slash");
        bool hasRattleGuard = actions.Contains("Rattle Guard");

        double healthPercentage = (double)self.Health / Math.Max(1, self.MaxHealth);
        if(healthPercentage < 0.3 && hasRattleGuard && Random.Shared.NextDouble() < 0.4)
        {
            return "Rattle Guard";
        }
        if(hasBoneSlash)
        {
            return "Bone Slash";
        }
        return GetDefaultAction(actions);
    }
}
