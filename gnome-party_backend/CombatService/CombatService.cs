using Amazon.DynamoDBv2.DataModel;
using GnomeParty.Database;
using Models;
using Models.Actions;
using Models.CharacterData;
using Models.CombatData;
using System.Runtime.InteropServices;
using System.Text.Json;


namespace CombatService
{
    public class CombatService
    {
        IDatabaseService databaseService;
        public CombatService() 
        {
           databaseService = new DatabaseService();
        }
        public CombatService(IDatabaseService databaseService)
        {
            this.databaseService = databaseService;
        }   

        public async Task<List<CombatResult>>  CombatRequestHandlerAsync(CombatRequest request)
        {
            
            var activeEncounter = await databaseService.LoadAsync<ActiveCombatEncounter>(request.EncounterId);

            //// Mark the source character as readied
            for (int i = 0; i < activeEncounter.GameState.PlayerCharacters.Count; i++)
            {
                if (activeEncounter.GameState.PlayerCharacters[i].Id == request.SourceCharacterId)
                {
                    activeEncounter.CombatRequests[i] = request;
                    activeEncounter.PlayerReadied[i] = true;
                    break;
                }
            }
            await databaseService.SaveAsync(activeEncounter);
            // Check if all players have readied up
            foreach (var playerReadier in activeEncounter.PlayerReadied)
            {
                if (!playerReadier)
                {
                    // Not all players have readied up yet, so we can't process the combat request
                    return [];
                }
            }
            // All players have readied up, so we can process all the combat requests

            var combatResults =   await ProcessCombatRequestsAsync(activeEncounter.CombatRequests.ToArray(), activeEncounter);
            var enemyCombatResquests = new List<CombatRequest>();
            foreach (var enemyCharacter in activeEncounter.GameState.EnemyCharacters)
            {
                var combatRequest = new Enemy(enemyCharacter).ChooseAction(activeEncounter.GameState.PlayerCharacters, activeEncounter.GameState.EnemyCharacters);
                enemyCombatResquests.Add(combatRequest);
            }
            var enemyCombatResults = await ProcessCombatRequestsAsync(enemyCombatResquests.ToArray(), activeEncounter);
            combatResults.AddRange(enemyCombatResults);
            return combatResults;
        }

        async Task<List<CombatResult>> ProcessCombatRequestsAsync(CombatRequest[] combatRequests, ActiveCombatEncounter encounter)
        {
            var combatResults = new List<CombatResult>();
            //save off the result of each combat request to the encounter, 
            // so it that each result can be sent back to the frontend
            foreach (var request in combatRequests)
            {
                var roundEvents = new List<CombatEvent>();
                var action = CharacterActionFactory.CreateCharacterAction(request.Action);
                //looks for character in
                var srcCharacter = encounter.GameState.PlayerCharacters.FirstOrDefault(c => c.Id == request.SourceCharacterId) ?? encounter.GameState.EnemyCharacters.FirstOrDefault(c => c.Id == request.SourceCharacterId);
                var targetCharacter = encounter.GameState.PlayerCharacters.FirstOrDefault(c => c.Id == request.TargetCharacterId) ?? encounter.GameState.EnemyCharacters.FirstOrDefault(c => c.Id == request.TargetCharacterId);
                var context = new AttackContext(srcCharacter, action, targetCharacter);
                action.ApplyEffect(srcCharacter, targetCharacter, context);

                // record the damage event of Player attacking the enemy
                roundEvents.Add(new CombatEvent("damage", new DamageEventParams {DamageAmount = context.ModifiedDamage, TargetId = targetCharacter.Id, SourceId = srcCharacter.Id, TargetName = targetCharacter.Name}));
                // Removes an enemy that has been defeated and prints a message about it
                var deathEvents = RemoveDeadCharacters(encounter.GameState);
                roundEvents.AddRange(deathEvents);
                var result = new CombatResult(request.DeepCopy(), encounter.GameState.DeepCopy(), roundEvents);
                combatResults.Add(result);
            }
            await databaseService.SaveAsync(encounter);
            return combatResults;
        }
        private List<CombatEvent> RemoveDeadCharacters(CombatEncounterGameState gameState)
        {
            var events = new List<CombatEvent>();
            var defeatedEnemies = gameState.EnemyCharacters.Where(c => c.Health <= 0).ToList();

            foreach (var enemy in defeatedEnemies)
            {
                events.Add(new CombatEvent("defeated", new DefeatedEventParams {TargetId = enemy.Id, TargetName = enemy.Name}));
            }

            gameState.EnemyCharacters.RemoveAll(c => c.Health <= 0);
            return events;
        }
    }
}