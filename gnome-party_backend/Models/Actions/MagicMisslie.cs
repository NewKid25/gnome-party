using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;

namespace Models.Actions
{
    public class MagicMisslie : CharacterAction
    {
        public MagicMisslie() : base("Magic Missile")
        {
            ActionDescription = new CharacterActionDescription("Magic Missile", "Deal 10 damage to target enemy uninterrupted");
        }
        public override AttackResolution ResolveAttack(Character user, Character target, CombatEncounterGameState gameState, bool isRedirected, bool unblockable)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (target == null) throw new ArgumentNullException(nameof(target));
            return new AttackResolution
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
                    }
                }
            };
        }
    }
}
