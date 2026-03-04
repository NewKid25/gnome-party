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

            Console.WriteLine($"Size of player characters is {activeEncounter.PlayerCharacters.Count}");
            Console.WriteLine($"Size of player readied is {activeEncounter.PlayerReadied.Length}");
            Console.WriteLine($"Size of combat requests is {activeEncounter.CombatRequests.Length}");
            // Mark the source character as readied
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

            Console.WriteLine($"encounter is {JsonSerializer.Serialize(activeEncounter)}");

            activeEncounter = await ProcessCombatRequestsAsync(activeEncounter.CombatRequests, activeEncounter);
            return activeEncounter;
            //if (request == null)
            //{
            //    return null;
            //}

            //Character_Base attacker = CreateCharacter(request.Attacker);
            //Character_Base target = CreateCharacter(request.Target);

            //if (attacker == null || target == null)
            //{
            //    return null;
            //}

            //CharacterAction attack = null;
            //foreach (CharacterAction a in attacker.Attacks)
            //{
            //    if (a.AttackName == request.Attack)
            //    {
            //        attack = a;
            //        break;
            //    }
            //}

            //if (attack == null)
            //{
            //    return null;
            //}

            //attacker.ResolveAttack(attack, target);

            //CombatResult result = new CombatResult();
            //result.AttackerId = attacker.CharacterID;
            //result.AttackerName = attacker.CharacterName;
            //result.TargetId = target.CharacterID;
            //result.TargetName = target.CharacterName;
            //result.TargetHealth = target.Health;

            //return result;
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

        private Character_Base CreateCharacter(PlayerCharacterClass type)
        {
            Guid id = Guid.NewGuid();

            if (type == PlayerCharacterClass.Warrior)
            {
                return new Warrior("Warrior", id);
            }
            else if (type == PlayerCharacterClass.Bard)
            {
                return new Bard("Bard", id);
            }
            else if (type == PlayerCharacterClass.Mage)
            {
                return new Mage("Mage", id);
            }
            else
            {
                return null;
            }
        }
        //public class CombatResult
        //{
        //    public Guid AttackerId { get; set; }
        //    public string AttackerName { get; set; }
        //    public Guid TargetId { get; set; }
        //    public string TargetName { get; set; }
        //    public int TargetHealth { get; set; }
        //}
    }
}