using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.Status;

namespace Models.Actions
{
    public sealed class Block : CharacterAction
    {
        public Block() : base("Block") 
        {
            ActionDescription = new CharacterActionDescription("Block", "Guard an ally");
        }
        public override void ApplyEffect(Character user, Character target, AttackContext context)
        {
            throw new NotImplementedException();
        }
        public override AttackResolution ResolveAttack(
            Character user, 
            Character ally, 
            CombatEncounterGameState gameState, 
            bool isRedirected = false, 
            bool isUnblockable = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (ally == null) throw new ArgumentNullException(nameof(ally));
            var resolution = new AttackResolution();
            resolution.StatusEffectsToApply.Add(new BlockStatus(user, ally));
            resolution.Events.Add(new CombatEvent("status_applied", new {statusType = StatusTypes.Block, sourceId = user.Id, ownerId = user.Id, targetId = ally.Id}));
            return resolution;
        }
    }
}
