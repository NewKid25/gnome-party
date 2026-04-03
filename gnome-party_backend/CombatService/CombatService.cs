using Amazon.DynamoDBv2.DataModel;
using GnomeParty.Database;
using Models;
using Models.CharacterData;
using Models.CombatData;
using Models.EncounterData;
using Models.Status;


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
        private void ApplyStatusEffects(Character character, StatusEffect newStatus)
        {
            var existingStatus = character.StatusEffects.FirstOrDefault(s => s.StatusType == newStatus.StatusType); 
            if (existingStatus == null)
            {
                character.StatusEffects.Add(newStatus);
                return;
            }
            existingStatus.SourceCharacterId = newStatus.SourceCharacterId;
            existingStatus.Duration = newStatus.Duration;
            existingStatus.DurationUnit = newStatus.DurationUnit;
            existingStatus.AffectedCharacterIds = new List<string>(newStatus.AffectedCharacterIds);
            existingStatus.ModifierValues = new Dictionary<string, double>(newStatus.ModifierValues);
            existingStatus.StatusDescription = new Dictionary<string, string>(newStatus.StatusDescription);
        }
        public async Task<List<CombatResult>> CombatRequestHandlerAsync(CombatRequest request)
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

            var combatResults = await ProcessCombatRequestsAsync(activeEncounter.CombatRequests.ToArray(), activeEncounter);
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
        private Character FindCharacter(CombatEncounterGameState gameState, string id)
        {
            Character character = gameState.PlayerCharacters.FirstOrDefault(c => c.Id == id);
            if (character != null)
            {
                return character;
            }
            character = gameState.EnemyCharacters.FirstOrDefault(c => c.Id == id);
            return character;
        }
        private IEnumerable<Character> GetAllCharacters(CombatEncounterGameState gameState)
        {
            return gameState.PlayerCharacters.Concat(gameState.EnemyCharacters);
        }
        private double GetDamageReduction(Character source, Character target, bool isUnblockable)
        {
            double reduction = 0.0;
            foreach (var status in target.StatusEffects)
            {
                reduction = status.ModifyDamageReduction(source, target, reduction, isUnblockable);
            }
            return Math.Min(reduction, 1.0);
        }
        private double GetIncomingDamageMultiplier(Character source, Character target, bool isUnblockable)
        {
            double multiplier = 1.0;
            foreach (var status in target.StatusEffects)
            {
                multiplier = status.ModifyIncomingDamageMultiplier(source, target, multiplier, isUnblockable);
            }
            return multiplier;
        }
        private double GetOutgoingDamageMultiplier(Character source, Character target, bool isUnblockable)
        {
            double multiplier = 1.0;
            foreach (var status in source.StatusEffects)
            {
                multiplier = status.ModifyOutgoingDamageMultiplier(source, target, multiplier, isUnblockable);
            }
            return multiplier;
        }
        async Task<List<CombatResult>> ProcessCombatRequestsAsync(CombatRequest[] combatRequests, ActiveCombatEncounter encounter)
        {
            var combatResults = new List<CombatResult>();
            //save off the result of each combat request to the encounter, 
            // so it that each result can be sent back to the frontend
            foreach (var request in combatRequests)
            {
                if(request == null)
                {
                    continue;
                }
                var roundEvents = new List<CombatEvent>();
                var action = CharacterActionFactory.CreateCharacterAction(request.Action);
                //looks for character in            
                var srcCharacter = FindCharacter(encounter.GameState, request.SourceCharacterId);
                var originalTargetCharacter = FindCharacter(encounter.GameState, request.TargetCharacterId);
                if (srcCharacter == null)
                {
                    throw new InvalidOperationException($"Source character '{request.SourceCharacterId}' was not found.");
                }
                if (originalTargetCharacter == null)
                {
                    throw new InvalidOperationException($"Target character '{request.TargetCharacterId}' was not found.");
                }
                ProcessStatusTriggers(encounter.GameState, srcCharacter, DurationUnit.TurnStart, roundEvents);
                var resolvedTarget = ResolveActionTarget(srcCharacter, action, encounter.GameState, originalTargetCharacter, action.Unblockable);
                var isRedirected = resolvedTarget.Id != originalTargetCharacter.Id;
                var resolution = action.ResolveAttack(srcCharacter, resolvedTarget, encounter.GameState, isRedirected);
                foreach (var attack in resolution.AttackInstances)
                {
                    var attackSource = FindCharacter(encounter.GameState, attack.SourceCharacterId);
                    var finalTarget = FindCharacter(encounter.GameState, attack.TargetCharacterId);
                    if (attackSource == null)
                    {
                        throw new InvalidOperationException("Attack source was not found.");
                    }
                    if (finalTarget == null)
                    {
                        throw new InvalidOperationException("Attack target was not found.");
                    }
                    var outgoingMultiplier = GetOutgoingDamageMultiplier(attackSource, finalTarget, attack.IsUnblockable);
                    var incomingMultiplier = GetIncomingDamageMultiplier(attackSource, finalTarget, attack.IsUnblockable);
                    var damageReduction = GetDamageReduction(attackSource, finalTarget, attack.IsUnblockable);
                    var finalDamage = (int)Math.Floor(
                        attack.BaseDamage *
                        outgoingMultiplier *
                        incomingMultiplier *
                        (1.0 - damageReduction)
                    );
                    if (finalDamage < 0)
                    {
                        finalDamage = 0;
                    }
                    attack.IsRedirected = isRedirected;
                    attack.FinalDamage = finalDamage;
                    attack.IsBlocked = damageReduction > 0;
                    finalTarget.Health -= finalDamage;
                    roundEvents.Add(new CombatEvent("damage", new DamageEventParams
                    {
                        DamageAmount = finalDamage,
                        TargetId = finalTarget.Id,
                        SourceId = attackSource.Id,
                        TargetName = finalTarget.Name
                    }));
                }
                foreach (var status in resolution.StatusEffectsToApply)
                {
                    var owner = FindCharacter(encounter.GameState, status.StatusOwnerCharacterId);
                    if (owner == null)
                    {
                        throw new InvalidOperationException("Status owner was not found.");
                    }
                    ApplyStatusEffects(owner, status);
                }
                roundEvents.AddRange(resolution.Events);
                roundEvents.AddRange(RemoveDeadCharacters(encounter.GameState));
                ProcessStatusTriggers(encounter.GameState, srcCharacter, DurationUnit.TurnEnd, roundEvents);
                var result = new CombatResult(request.DeepCopy(), encounter.GameState.DeepCopy(), roundEvents);
                combatResults.Add(result);
            }
            await databaseService.SaveAsync(encounter);
            return combatResults;
        }
        public void ProcessStatusTriggers(CombatEncounterGameState gameState, Character character, DurationUnit trigger, List<CombatEvent> events)
        {
            var expiredStatuses = new List<StatusEffect>();
            foreach (var status in character.StatusEffects.Where(s => s.DurationUnit == trigger).ToList())
            {
                status.Process(character, events);
                status.Duration--;
                if (status.Duration <= 0)
                {
                    expiredStatuses.Add(status);
                }
            }
            foreach (var expiredStatus in expiredStatuses)
            {
                character.StatusEffects.Remove(expiredStatus);
                events.Add(new CombatEvent("StatusExpired", new StatusExpiredEventParams
                {
                    StatusType = expiredStatus.StatusType,
                    OwnerId = character.Id
                }));
            }
        }
        List<CombatEvent> RemoveDeadCharacters(CombatEncounterGameState gameState)
        {
            var events = new List<CombatEvent>();
            var defeatedEnemies = gameState.EnemyCharacters.Where(c => c.Health <= 0).ToList();

            foreach (var enemy in defeatedEnemies)
            {
                events.Add(new CombatEvent("defeated", new DefeatedEventParams { TargetId = enemy.Id, TargetName = enemy.Name }));
            }

            gameState.EnemyCharacters.RemoveAll(c => c.Health <= 0);
            return events;
        }
        private Character ResolveActionTarget(
            Character source,
            CharacterAction action,
            CombatEncounterGameState gameState,
            Character originalTarget,
            bool isUnblockable)
        {
            Character resolvedTarget = originalTarget;
            foreach (var character in gameState.PlayerCharacters.Concat(gameState.EnemyCharacters))
            {
                foreach (var status in character.StatusEffects)
                {
                    resolvedTarget = status.ModifyRedirectTarget(
                        source,
                        originalTarget,
                        resolvedTarget,
                        gameState,
                        isUnblockable);
                }
            }
            return resolvedTarget;
        }
    }
}