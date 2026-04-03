using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions
{
    public sealed class RattleGuard : CharacterAction
    {
        public RattleGuard() : base("Rattle Guard")
        {
            ActionDescription = new CharacterActionDescription("Rattle Guard", "Reduce damage by 50% for one turn");
        }
        public override AttackResolution ResolveAttack(
            Character user, 
            Character ally, 
            CombatEncounterGameState gameState, 
            bool isRedirected = false, 
            bool isUnblockable = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var resolution = new AttackResolution();
            resolution.StatusEffectsToApply.Add(new RattleGuardStatus(user));
            resolution.Events.Add(new CombatEvent("status_applied", new { statusType = StatusTypes.RattleGuard, sourceId = user.Id, ownerId = user.Id, targetId = user.Id }));
            return resolution;
        }
    }
}
