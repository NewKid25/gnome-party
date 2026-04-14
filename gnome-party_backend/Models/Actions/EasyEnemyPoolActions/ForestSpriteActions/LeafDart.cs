using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CombatData;
using Models.TestHelperData;

namespace Models.Actions.EasyEnemyPoolActions.ForestSpriteActions
{
    // Leaf Dart: Deal four damage twice randomly
    public sealed class LeafDart : CharacterAction
    {
        private readonly IRandomGenerator rng;
        public LeafDart() : this(new RandomNumGen()) { }
        public LeafDart(IRandomGenerator rng) : base("Leaf Dart")
        {
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            this.rng = rng;
            ActionDescription = new CharacterActionDescription("Leaf Dart", "Do double hits of 4 damage randomly");
        }

        public override AttackResolution ResolveAttack(
            Character user, 
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected, 
            bool isUnblockable)
        {
            if (user == null) throw new ArgumentNullException(nameof(user)); // Validate that the user character is not null
            if (target == null) throw new ArgumentNullException(nameof(target)); // Validate that the target character is not null
            if (gameState == null) throw new ArgumentNullException(nameof(gameState)); // Validate that the gameState is not null

            var resolution = new AttackResolution(); // Creare a new attack resolution to hold the results of the attack

            // Validate that the target is eligible for this attack
            List<Character> eligibleTargets = ReturnEligibleTargets(user, gameState);
            if (!eligibleTargets.Any(c => c.Id == target.Id)) { throw new ArgumentException("Target is not eligible for this attack", nameof(target)); }

            int hitCount = 2; // Number of Leaf Darts

            // Randomly target hitCount number of unique targets (double attack same target if only 1 target is avaliable)
            if (eligibleTargets.Count == 1)
            {
                var onlyTarget = eligibleTargets[0];

                for (int i = 0; i < hitCount; i++)
                {
                    resolution.AttackInstances.Add(new AttackInstance
                    {
                        ActionName = AttackName,
                        BaseDamage = 4,
                        FinalDamage = 4,
                        SourceCharacterId = user.Id,
                        TargetCharacterId = onlyTarget.Id,
                        IsRedirected = isRedirected,
                        IsUnblockable = isUnblockable
                    });
                }
                return resolution;
            }

            for (int i = 0; i < hitCount; i++)
            {
                int randomIndex = (int)(rng.NextDouble() * eligibleTargets.Count);
                Character randomTarget = eligibleTargets[randomIndex];
                resolution.AttackInstances.Add(new AttackInstance
                {
                    ActionName = AttackName,
                    BaseDamage = 4,
                    FinalDamage = 4,
                    SourceCharacterId = user.Id,
                    TargetCharacterId = randomTarget.Id,
                    IsRedirected = isRedirected,
                    IsUnblockable = isUnblockable
                });
            }

            return resolution;
        }
    }
}
