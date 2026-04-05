using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Actions
{
    // Magic Missile: Deal 10 damage to target enemy uninterrupted
    public class MagicMisslie : CharacterAction
    {
        public MagicMisslie() : base("Magic Missile") // Pass the action name to the base constructor
        {
            ActionDescription = new CharacterActionDescription("Magic Missile", "Deal 10 damage to target enemy uninterrupted"); // Set the action description
        }
        public override AttackResolution ResolveAttack( // Override the ResolveAttack method to implement the specific logic for Magic Missile
            Character user, 
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected, 
            bool unblockable)
        {
            if (user == null) throw new ArgumentNullException(nameof(user)); // Validate that the user character is not null
            if (target == null) throw new ArgumentNullException(nameof(target)); // Validate that the target character is not null
            return new AttackResolution // Return a new AttackResolution object with the details of the attack
            {
                AttackInstances = new List<AttackInstance>
                {
                    new AttackInstance
                    {
                        ActionName = AttackName,
                        BaseDamage = 10,
                        FinalDamage = 10,
                        SourceCharacterId = user.Id,
                        TargetCharacterId = target.Id,
                        IsUnblockable = true,
                        IsBlocked = false,
                    }
                }
            };
        }
    }
}
