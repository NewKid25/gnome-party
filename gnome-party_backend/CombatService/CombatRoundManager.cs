using System;

namespace CombatService
{
    public sealed class CombatRoundManager
    {
        private readonly Random _random;
        public CombatRoundManager(Random rand)
        {
            if (rand == null)
            { 
                _random = new Random(); 
            }
            else
            {
                _random = rand;
            }
        }
        public CombatRoundManager() : this(null) { }
        public void ExecuteRound(List<Character_Base> playerParty, List<Character_Base> enemyGroup, List<PlannedAction> playerPlannedActions)
        {
            if (playerParty == null) throw new ArgumentNullException("playerParty");
            if (enemyGroup == null) throw new ArgumentNullException("enemyGroup");
            if(playerPlannedActions == null)
            {
                playerPlannedActions = new List<PlannedAction>();
            }
            ConnectOpponentsToPlayers(playerParty, enemyGroup);
            CallRoundStart(playerParty);
            CallRoundStart(enemyGroup);
            List<PlannedAction> enemyActions = BuildEnemyActions(playerParty, enemyGroup);
            ResolveActions(playerPlannedActions);
            ResolveActions(enemyActions);
            CallRoundEnd(playerParty);
            CallRoundEnd(enemyGroup);
        }
        private void ConnectOpponentsToPlayers(List<Character_Base> playerList, List<Character_Base> enemiesGroup)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                Character_Base player = playerList[i];
                if(player == null) continue;
                player.Opponents = enemiesGroup;
            }
            for (int i = 0; i < enemiesGroup.Count; i++)
            {
                Character_Base enemy = enemiesGroup[i];
                if(enemy == null) continue;
                enemy.Opponents = playerList;
            }
        }
        private void CallRoundStart(List<Character_Base> group)
        {
            for(int i = 0; i < group.Count; i++)
            {
                Character_Base character = group[i];
                if(character != null && character.IsAlive) { character.OnRoundStart(); }
            }
        }
        private void CallRoundEnd(List<Character_Base> group)
        {
            for (int i = 0; i < group.Count; i++)
            {
                Character_Base character = group[i];
                if (character != null && character.IsAlive) { character.OnRoundEnd(); }
            }
        }
        private List<PlannedAction> BuildEnemyActions(List<Character_Base> playerList, List<Character_Base> enemyGroup)
        {
            List<PlannedAction> plannedActions = new List<PlannedAction>();
            for (int i = 0; i < enemyGroup.Count; i++)
            {
                Character_Base enemy = enemyGroup[i];
                if (enemy == null || !enemy.IsAlive) continue;

                PlannedAction action = enemy.ChooseEnemyAction(playerList, enemyGroup, _random);
                if (action != null) { plannedActions.Add(action); }
            }
            return plannedActions;
        }
        private void ResolveActions(List<PlannedAction> actions)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                PlannedAction action = actions[i];
                if (action == null) continue;
                Character_Base user = action.User;
                if(user == null || !user.IsAlive) continue;
                user.ResolvePlannedAction(action);
            }
        }
    }
}
