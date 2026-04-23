using Models.Actions;
using Models.Actions.BossPoolActions.NecrognomancerActions;

namespace Models.CharacterData.BossEnemyPoolClasses
{
    public class Necrognomancer : Character
    {
        public int ActiveSummonCount { get; set; } = 0;
        public int TurnCount = 0;
        public Necrognomancer()
        {
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new DarkBolt().ActionDescription,
                new SoulDrain().ActionDescription,
                new Summon().ActionDescription,
            };
            CharacterType = "Necrognomancer";
            Health = 40;
            MaxHealth = 40;
            Name = "Necrognomancer";
        }
        public override Character DeepCopy()
        {
            return new Necrognomancer
            {
                Id = Id,
                Name = Name,
                CharacterType = CharacterType,
                Health = Health,
                MaxHealth = MaxHealth,
                ActiveSummonCount = ActiveSummonCount,
                TurnCount = TurnCount,
                ActionsDescriptions = new List<CharacterActionDescription>(ActionsDescriptions),
                StatusEffects = StatusEffects.Select(s => s.DeepCopy()).ToList(),
            };
        }
    }
}
