using Amazon.DynamoDBv2.DataModel;
using GnomeParty.Database;
using Models;
using Models.ActionMetaData;
using Models.Actions;
using Models.CharacterData;
using Models.CombatData;
using Models.EncounterData;
using Models.Status;
using Models.TestHelperData;


namespace CombatService
{
    public class CombatService
    {
        IDatabaseService databaseService;
        private readonly IRandomGenerator rng;
        public CombatService()
        {
            databaseService = new DatabaseService();
            rng = new RandomNumGen();
        }
        public CombatService(IDatabaseService databaseService, IRandomGenerator rng)
        {
            this.databaseService = databaseService;

            if(rng == null)
            {
                throw new ArgumentNullException(nameof(rng));
            }
            this.rng = rng;
        }

        // Method for refreshing or adding a new copy of a status effect
        // * Status effects do not stack, they simply refresh their duration and modifiers *
        private void ApplyStatusEffects(Character character, StatusEffect newStatus)
        {
            // Checks for an existing copy of a status. Makes a new status if 
            // there isn't one, otherwise refreshes the existing one.
            var existingStatus = character.StatusEffects.FirstOrDefault(s => s.GetType() == newStatus.GetType());
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
        
        // Method for finding a character (player or enemy) in the game state
        private Character FindCharacter(CombatEncounterGameState gameState, string id)
        {
            // Search for a character by id in either the player or enemy list
            Character character = gameState.PlayerCharacters.FirstOrDefault(c => c.Id == id);
            if (character != null)
            {
                return character;
            }
            character = gameState.EnemyCharacters.FirstOrDefault(c => c.Id == id);
            return character;
        }

        // Method for retreiving the eligible targets for a given character
        public List<ActionTargetInfo> GetActionTargets(string encounterId, string characterId)
        {
            // Gets the encounter and character associated with the ids
            var encounter = databaseService.LoadAsync<ActiveCombatEncounter>(encounterId).Result;
            var character = FindCharacter(encounter.GameState, characterId);

            if(character == null) { throw new InvalidOperationException("Character was not found."); } // Verify the character is no null

            var results = new List<ActionTargetInfo>(); // variable to store the eligible action targets

            foreach(var actionDescription in character.ActionsDescriptions)
            {
                // Find the action and targets associated with that action
                var action = CharacterActionFactory.CreateCharacterAction(actionDescription.Name);
                var eligibleTargets = action.ReturnEligibleTargets(character, encounter.GameState);

                results.Add(new ActionTargetInfo
                {
                    ActionName = action.AttackName,
                    EligibleTarget = eligibleTargets.Select(c => new TargetInfo
                    {
                        CharacterId = c.Id,
                        Name = c.Name,
                        CharacterType = c.CharacterType,
                        Health = c.Health,
                        MaxHealth = c.MaxHealth
                    }).ToList()
                });
            }
            return results;
        }

        // Method for getting all characters (player and enemy) in the game state
        private IEnumerable<Character> GetAllCharacters(CombatEncounterGameState gameState)
        {
            return gameState.PlayerCharacters.Concat(gameState.EnemyCharacters);
        }

        // Method for calculating damage reduction modifiers from status effects
        private double GetDamageReduction(Character source, Character target, bool isUnblockable)
        {
            // Iterate through all status effects on the target to find damage reduction modifiers
            double reduction = 0.0;
            foreach (var status in target.StatusEffects)
            {
                reduction = status.ModifyDamageReduction(source, target, reduction, isUnblockable);
            }
            return Math.Min(reduction, 1.0);
        }

        // Method for calculating incoming damage multiplier from status effects
        private double GetIncomingDamageMultiplier(Character source, Character target, bool isUnblockable)
        {
            // Iterate through all status effects on the target to find incoming damage multiplier modifiers
            double multiplier = 1.0;
            foreach (var status in target.StatusEffects)
            {
                multiplier = status.ModifyIncomingDamageMultiplier(source, target, multiplier, isUnblockable);
            }
            return multiplier;
        }

        // Method for calculating outgoing damage multiplier from status effects
        private double GetOutgoingDamageMultiplier(Character source, Character target, bool isUnblockable)
        {
            // Iterate through all status effects on the source to find outgoing damage multiplier modifiers
            double multiplier = 1.0;
            foreach (var status in source.StatusEffects)
            {
                multiplier = status.ModifyOutgoingDamageMultiplier(source, target, multiplier, isUnblockable);
            }
            return multiplier;
        }
        
        // Method for processing combat requests and returning the results
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

                // Variables to hold the combat state and events
                var roundEvents = new List<CombatEvent>();
                var action = CharacterActionFactory.CreateCharacterAction(request.Action);

                //looks for characters and null checks them        
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

                // Check if the srcCharacter is stunned
                var isStunned = srcCharacter.StatusEffects.OfType<StunStatus>().Any();
                if(isStunned)
                {
                    ProcessStatusTriggers(encounter.GameState, srcCharacter, DurationUnit.TurnStart, roundEvents);
                    roundEvents.Add(new CombatEvent("stunned", new 
                    { 
                        sourceId = srcCharacter.Id,
                        targetId = srcCharacter.Id,
                    }));
                    var stunnedResults = new CombatResult(request.DeepCopy(), encounter.GameState.DeepCopy(), roundEvents);
                    combatResults.Add(stunnedResults);
                    continue;
                }

                ProcessStatusTriggers(encounter.GameState, srcCharacter, DurationUnit.TurnStart, roundEvents); // Process status triggers that happen at the beginning of a character's turn
                var resolvedTarget = ResolveActionTarget(srcCharacter, action, encounter.GameState, originalTargetCharacter, action.Unblockable); // Resolve any target changes
                var isRedirected = resolvedTarget.Id != originalTargetCharacter.Id; // Create a variable to determine if an attack has been redirected
                var resolution = action.ResolveAttack(srcCharacter, resolvedTarget, encounter.GameState, isRedirected); // Get the action instance with status effects applied
                resolution = ResolveMirror(encounter.GameState, srcCharacter, action, request.Action, resolution); // Run a second copy of the action instance to the target associated with the mirror status
                foreach (var attack in resolution.AttackInstances) // iterate through each attack instance in the resolution
                {
                    // Find and null check who is attacking and who is receiving that attack
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

                    // Calculating final damage after all modifiers
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

                // Iterate through each status effect in the resolution and apply it to the appropriate character
                foreach (var status in resolution.StatusEffectsToApply)
                {
                    var owner = FindCharacter(encounter.GameState, status.StatusOwnerCharacterId);
                    if (owner == null)
                    {
                        throw new InvalidOperationException("Status owner was not found.");
                    }
                    ApplyStatusEffects(owner, status);
                }

                // Iterate through each instance of healing to process and apply it to the appropriate character
                foreach(var heal in resolution.HealInstances)
                {
                    // Null check for healing source and target
                    var healingSource = FindCharacter(encounter.GameState, heal.SourceCharacterId);
                    var healingTarget = FindCharacter(encounter.GameState, heal.TargetCharacterId);
                    if(healingSource == null) { throw new InvalidOperationException("Healing source was not found."); }
                    if(healingTarget == null) { throw new InvalidOperationException("Healing target was not found."); }

                    // Calculate final healing after all modifiers
                    var finalHealing = heal.BaseHealing;
                    heal.FinalHealing = finalHealing;

                    HealCharacter(healingTarget, finalHealing); // Apply the healing to the target

                    roundEvents.Add(new CombatEvent("healed", new
                    {
                        SourceId = healingSource.Id,
                        TargetId = healingTarget.Id,
                        TargetName = healingTarget.Name,
                        HealingAmount = finalHealing
                    }));
                }

                roundEvents.AddRange(resolution.Events); // Store events from the given round/turn
                roundEvents.AddRange(RemoveDeadCharacters(encounter.GameState)); // Remove enemies that have died
                ProcessStatusTriggers(encounter.GameState, srcCharacter, DurationUnit.TurnEnd, roundEvents); // Process any status effects that happen at the end of their turn (after they've attacked)
                
                // Produce the final result to send to the client
                var result = new CombatResult(request.DeepCopy(), encounter.GameState.DeepCopy(), roundEvents);
                combatResults.Add(result);
            }
            await databaseService.SaveAsync(encounter);
            return combatResults; // Return the results
        }
        
        // Method for processing status triggers (effects that occur at the start or end of a turn)
        public void ProcessStatusTriggers(CombatEncounterGameState gameState, Character character, DurationUnit trigger, List<CombatEvent> events)
        {
            // Iterate through all status effects on the character, update duration, and remove expired statuses
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
                var name = expiredStatus.GetType().Name.Replace("Status", "").ToLower();
                events.Add(new CombatEvent($"{name}_expired", new StatusExpiredEventParams
                {
                    CharacterId = character.Id
                }));
            }
        }
        
        // Method for removing dead *ENEMY* characters from the game state
        List<CombatEvent> RemoveDeadCharacters(CombatEncounterGameState gameState)
        {
            // Iterate through all enemy characters and remove those that have been defeated
            var events = new List<CombatEvent>();
            var defeatedEnemies = gameState.EnemyCharacters.Where(c => c.Health <= 0).ToList();

            foreach (var enemy in defeatedEnemies)
            {
                events.Add(new CombatEvent("defeated", new DefeatedEventParams { TargetId = enemy.Id, TargetName = enemy.Name }));
            }

            gameState.EnemyCharacters.RemoveAll(c => c.Health <= 0);
            return events;
        }
        
        // Method for resolving the target of an action, taking redirection from status effects into account
        private Character ResolveActionTarget(
            Character attacker,
            CharacterAction action,
            CombatEncounterGameState gameState,
            Character originalTarget,
            bool isUnblockable)
        {
            // Iterate through all characters to check for redirecting status effects
            Character resolvedTarget = originalTarget;
            foreach (var character in gameState.PlayerCharacters.Concat(gameState.EnemyCharacters))
            {
                foreach (var status in character.StatusEffects)
                {
                    resolvedTarget = status.ModifyRedirectTarget(
                        attacker,
                        originalTarget,
                        resolvedTarget,
                        gameState,
                        isUnblockable);
                }
            }
            return resolvedTarget;
        }
        
        // Method for resolving the mirror status effect
        private AttackResolution ResolveMirror(
            CombatEncounterGameState gameState, 
            Character character, 
            CharacterAction action, 
            string actionName,
            AttackResolution targetResolution)
        {
            // Null checks
            if (gameState == null) { throw new ArgumentNullException(nameof(gameState)); }
            if(character == null) { throw new ArgumentNullException(nameof(character)); }
            if(action == null) { throw new ArgumentNullException(nameof(action)); }
            if(targetResolution == null) { throw new ArgumentNullException(nameof(targetResolution)); }

            if(string.IsNullOrEmpty(actionName) || actionName == "Mirror") { return targetResolution; } // Checking for null or repeat mirror to prevent infinite loops

            // Check if the character has a mirror status effect
            var mirrorStatus = character.StatusEffects.OfType<MirrorStatus>().FirstOrDefault(); 
            if (mirrorStatus == null) { return targetResolution; }

            // Get the target id of the mirror effect
            var mirrorTargetId = mirrorStatus.AffectedCharacterIds.FirstOrDefault();
            if(string.IsNullOrEmpty(mirrorTargetId)) { return targetResolution; }

            // Find the mirror target character
            var mirrorTarget = FindCharacter(gameState, mirrorTargetId);
            if (mirrorTarget == null || mirrorTarget.Health <= 0) { return targetResolution; }

            // Resolve the mirror action as if the character had targeted the mirror target instead.
            var resolvedMirrorTarget = ResolveActionTarget(character, action, gameState, mirrorTarget, action.Unblockable);
            var mirrorIsRedirected = resolvedMirrorTarget.Id != mirrorTarget.Id;
            var mirrorAction = action.ResolveAttack(character, resolvedMirrorTarget, gameState, mirrorIsRedirected, action.Unblockable);
            mirrorAction.Events.Add(new CombatEvent("mirror_activated", new  // This event can be used by the frontend to trigger mirror-specific animations or effects
            {
                sourceId = character.Id,
                targetId = mirrorTarget.Id,
                actionName = actionName
            }));

            // Combine the mirror action resolution with the original target resolution
            targetResolution.AttackInstances.AddRange(mirrorAction.AttackInstances);
            targetResolution.StatusEffectsToApply.AddRange(mirrorAction.StatusEffectsToApply);
            targetResolution.Events.AddRange(mirrorAction.Events);

            return targetResolution; // The original resolution is returned but with the mirror action's effects combined in. 
        }

        // Method for healing a character
        public void HealCharacter(Character character, int amount)
        {
            // Null check character for healing and healing amount
            if(character == null) { throw new ArgumentNullException(nameof(character)); }
            if(amount < 0) { throw new ArgumentOutOfRangeException(nameof(amount)); }

            if(character.Health + amount >= character.MaxHealth) {  character.Health = character.MaxHealth; } // Check if the character would be over-healed
            else // Heal character normally
            {
                character.Health += amount;
            }
        }

    }
}