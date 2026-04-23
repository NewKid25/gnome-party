using Models.Actions;
using Models.Actions.EasyEnemyPoolActions.ForestSpriteActions;
using Models.Actions.EasyEnemyPoolActions.GoblinArcherActions;
using Models.Actions.EasyEnemyPoolActions.SkeletonActions;

namespace Models.CharacterData.BossEnemyPoolClasses.Summons
{
    public class Summons : Character
    {
        public Summons(SummonType type)
        {
            Id = Guid.NewGuid().ToString();

            switch (type)
            {
                case SummonType.Skeleton:
                    ActionsDescriptions = new List<CharacterActionDescription>
                    {
                        new BoneSlash().ActionDescription,
                        new RattleGuard().ActionDescription,
                    };
                    CharacterType = "Skeleton";
                    Health = 10;
                    MaxHealth = 10;
                    Name = "Skeleton";
                    break;

                case SummonType.ForestSprite:
                    ActionsDescriptions = new List<CharacterActionDescription>
                    {
                        new LeafDart().ActionDescription,
                        new Entangle().ActionDescription,
                    };
                    CharacterType = "Forest Sprite";
                    Health = 10;
                    MaxHealth = 10;
                    Name = "Forest Sprite";
                    break;

                case SummonType.GoblinArcher:
                    ActionsDescriptions = new List<CharacterActionDescription>
                    {
                        new PiercingArrow().ActionDescription,
                        new CripplingShot().ActionDescription,
                    };
                    CharacterType = "Goblin Archer";
                    Health = 10;
                    MaxHealth = 10;
                    Name = "Goblin Archer";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}