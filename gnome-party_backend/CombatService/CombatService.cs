using Amazon.DynamoDBv2.DataModel;
using GnomeParty.Database;
using GnomeParty.Models;
using System.Text.Json;


namespace GnomeParty.Combat
{
    public class CombatService
    {
        public async Task<ActiveCombatEncounter>  CombatRequestHandlerAsync(CombatRequest request)
        {
            var databaseService = new DatabaseService();
            var activeEncounter = await databaseService.LoadAsync<ActiveCombatEncounter>(request.EncounterId);

            //// Mark the source character as readied
            for (int i = 0; i < activeEncounter.PlayerCharacters.Count; i++)
            {
                if (activeEncounter.PlayerCharacters[i].Id == request.SourceCharacterId)
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
                    return activeEncounter;
                }
            }
            // All players have readied up, so we can process all the combat requests

            //Console.WriteLine($"encounter is {JsonSerializer.Serialize(activeEncounter)}");

            activeEncounter = await ProcessCombatRequestsAsync(activeEncounter.CombatRequests, activeEncounter);
            return activeEncounter;
        }

        async Task<ActiveCombatEncounter> ProcessCombatRequestsAsync(CombatRequest[] combatRequests, ActiveCombatEncounter encounter)
        {
            foreach (var request in combatRequests)
            {
                var action = CharacterActionFactory.CreateCharacterAction(request.Action);
                var srcCharacter = encounter.PlayerCharacters.FirstOrDefault(c => c.Id == request.SourceCharacterId) ?? encounter.EnemyCharacters.FirstOrDefault(c => c.Id == request.SourceCharacterId);
                var targetCharacter = encounter.PlayerCharacters.FirstOrDefault(c => c.Id == request.TargetCharacterId) ?? encounter.EnemyCharacters.FirstOrDefault(c => c.Id == request.TargetCharacterId);
                var context = new AttackContext(srcCharacter, action, targetCharacter);
                action.ApplyEffect(srcCharacter, targetCharacter, context);
            }
            await new DatabaseService().SaveAsync(encounter);
            return encounter;
        }
    }
}