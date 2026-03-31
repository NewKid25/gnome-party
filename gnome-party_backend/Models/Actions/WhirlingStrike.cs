using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;

namespace Models.Actions
{
    public class WhirlingStrike : CharacterAction
    {
        public WhirlingStrike() : base("Whirling Strike")
        {
            ActionDescription = new CharacterActionDescription("Whirling Strike", "Deal 5 damage to all enemies");
        }
        public override void ApplyEffect(Character user, Character target, AttackContext context)
        {
            throw new NotImplementedException();
        }
        public override AttackResolution ResolveAttack(Character user, Character target, CombatEncounterGameState gameState, bool isRedirected = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (target == null) throw new ArgumentNullException(nameof(target));
            List<Character> whirlStrikeTargets;
            if(isRedirected)
            {
                whirlStrikeTargets = new List<Character> { target };
            }
            else
            {
                whirlStrikeTargets = TargetingService.GetOpposingTeam(gameState, user.Id);
            }
            var resolution = new AttackResolution();
            for(int i = 0; i < whirlStrikeTargets.Count; i++)
            {
                resolution.AttackInstances.Add(new AttackInstance
                {
                    SourceCharacterId = user.Id,
                    TargetCharacterId = target.Id,
                    ActionName = AttackName,
                    BaseDamage = 5,
                    FinalDamage = 5,
                    IsRedirected = isRedirected
                });
            }
            return resolution;
        }
    }
}
