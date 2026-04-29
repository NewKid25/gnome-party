using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Events;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Models.Actions
{
    // Base class for character actions
    public abstract class CharacterAction
    {
        public string AttackName { get; private set; }
        public bool Unblockable { get; private set; }
        public bool UnRedirectable { get; private set; }
        protected CharacterAction(string attackName) : this(attackName, false, false) { }
        protected CharacterAction(string attackName, bool unblockable, bool unredirectable)
        {
            AttackName = attackName;
            Unblockable = unblockable;
            UnRedirectable = unredirectable;
            ActionDescription = new CharacterActionDescription(attackName);
        }
        public CharacterActionDescription ActionDescription { get; set; }
        public abstract AttackResolution ResolveAttack(Character user, Character target, CombatEncounterGameState gameState, bool isRedirected = false, bool isUnblockable = false);
        public virtual List<Character> ReturnEligibleTargets(Character user, CombatEncounterGameState gameState)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));
            return TargetingService.GetOpposingTeam(gameState, user.Id);
        }
        //used by the bard actions to change to a new action after being used, and then emit an event to update the client with the new action
        protected internal void ReplaceActionInUser(Character user, CharacterAction actionToReplace, CharacterAction newAction)
        {
            // place new action in same slot as this action, by swapping out at same index
            var actionIndex = user.ActionsDescriptions.IndexOf(actionToReplace.ActionDescription);
            Console.WriteLine($"Replacing action at index {actionIndex} with new action {newAction.AttackName}");
            Console.WriteLine($"User's actions before replacement: {JsonSerializer.Serialize(user.ActionsDescriptions)}");
            user.ActionsDescriptions.RemoveAt(actionIndex);
            user.ActionsDescriptions.Insert(actionIndex, newAction.ActionDescription);
            Console.WriteLine($"User's actions after replacement: {JsonSerializer.Serialize(user.ActionsDescriptions)}");
            // Emit event with updated character
            SocketEvents.RaiseActionUpdated(user);
        }
    }
}
