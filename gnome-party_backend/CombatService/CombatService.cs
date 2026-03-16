using Amazon.DynamoDBv2.DataModel;
using GnomeParty.Database;
using GnomeParty.Models;
using Models;
using System.Runtime.InteropServices;
using System.Text.Json;


namespace GnomeParty.Combat
{
    public class CombatService
    {
        public async Task<List<CombatResult>>  CombatRequestHandlerAsync(CombatRequest request)
        {
            var databaseService = new DatabaseService();
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
                    return [new CombatResult(request, activeEncounter.GameState)];
                }
            }
            // All players have readied up, so we can process all the combat requests

           var CombatResult =   await ProcessCombatRequestsAsync(activeEncounter.CombatRequests, activeEncounter);
            return CombatResult;
        }

        async Task<List<CombatResult>> ProcessCombatRequestsAsync(CombatRequest[] combatRequests, ActiveCombatEncounter encounter)
        {
            var combatRequestGameStateTuples = new List<CombatResult>();
            //save off the result of each combat request to the encounter, 
            // so it that each result can be send back to the 
            foreach (var request in combatRequests)
            {
                var action = CharacterActionFactory.CreateCharacterAction(request.Action);
                var srcCharacter = encounter.GameState.PlayerCharacters.FirstOrDefault(c => c.Id == request.SourceCharacterId) ?? encounter.GameState.EnemyCharacters.FirstOrDefault(c => c.Id == request.SourceCharacterId);
                var targetCharacter = encounter.GameState.PlayerCharacters.FirstOrDefault(c => c.Id == request.TargetCharacterId) ?? encounter.GameState.EnemyCharacters.FirstOrDefault(c => c.Id == request.TargetCharacterId);
                var context = new AttackContext(srcCharacter, action, targetCharacter);
                action.ApplyEffect(srcCharacter, targetCharacter, context);         
                // Removes an enemy that has been defeated and prints a message about it
                var messages = RemoveDeadCharacters(encounter.GameState);
                var result = new CombatResult(request.DeepCopy(), encounter.GameState.DeepCopy());
                result.Messages.AddRange(messages);
                combatRequestGameStateTuples.Add(result);
                //combatRequestGameStateTuples.Add(new CombatResult(request.DeepCopy(), encounter.GameState.DeepCopy())); //make sure al the info is a copy so that character data is not changed in this states during future iterations of this loop
            }
            await new DatabaseService().SaveAsync(encounter);
            return combatRequestGameStateTuples;
        }
        private List<string> RemoveDeadCharacters(CombatEncounterGameState gameState)
        {
            var messages = new List<string>();
            var defeatedEnemies = gameState.EnemyCharacters.Where(c => c.Health <= 0).ToList();

            foreach (var enemy in defeatedEnemies)
            {
                messages.Add($"{enemy.Name} has been defeated!");
            }

            gameState.EnemyCharacters.RemoveAll(c => c.Health <= 0);
            return messages;
        }
    }
}