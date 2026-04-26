using Models.Actions;
using Models.Actions.DifficultEnemyPoolActions.GnombieBruteActions;

namespace Models.CharacterData.DifficultEnemyPoolClasses
{
    public class GnombieBrute : Character
    {
        public int turnCount = 0;
        public GnombieBrute()
        {
            // List of actions available to the Cave Bat
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new HeavySlam().ActionDescription,
                new RottingAura().ActionDescription,
            };
            CharacterType = "Gnombie Brute";
            Health = 30;
            Id = Guid.NewGuid().ToString();
            MaxHealth = 30;
            Name = "Gnombie Brute";
            turnCount = 0;
        }
        public override Character DeepCopy()
        {
            return new GnombieBrute
            {
                Id = Id,
                Name = Name,
                CharacterType = CharacterType,
                Health = Health,
                MaxHealth = MaxHealth,
                turnCount = turnCount,
                ActionsDescriptions = new List<CharacterActionDescription>(ActionsDescriptions),
                StatusEffects = StatusEffects.Select(s => s.DeepCopy()).ToList(),
            };
        }
    }
}
