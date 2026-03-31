using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Actions
{
    public sealed class FuryStrikes : CharacterAction
    {
        private readonly int hitCount;
        public FuryStrikes() : this(Random.Shared.Next(2, 5))
        {
        }
        public FuryStrikes(int hitCount) : base("Fury Strikes")
        {
            if (hitCount < 2 || hitCount > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(hitCount));
            }
            this.hitCount = hitCount;
            ActionDescription = new CharacterActionDescription(
                "Fury Strikes",
                "Hit the same target 2 to 4 times for 3 damage each"
            );
        }
        public override void ApplyEffect(Character user, Character target, AttackContext context)
        {
            throw new NotImplementedException();
        }
        public override AttackResolution ResolveAttack(Character user, Character target, CombatEncounterGameState gameState, bool isRedirected = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (target == null) throw new ArgumentNullException(nameof(target));
            var resolution = new AttackResolution();
            for (int i = 0; i < hitCount; i++)
            {
                resolution.AttackInstances.Add(new AttackInstance
                {
                    SourceCharacterId = user.Id,
                    TargetCharacterId = target.Id,
                    ActionName = AttackName,
                    BaseDamage = 3,
                    FinalDamage = 3
                });
            }
            return resolution;
        }
    }
}
