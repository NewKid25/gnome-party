using Models.Actions;
using Models.Actions.EasyEnemyPoolActions.SkeletonActions;

namespace Models.CharacterData.EasyEnemyPoolClasses;
public class Skeleton : Character
{
    public Skeleton()
    {
        // List of actions available to the Skeleton
        ActionsDescriptions = new List<CharacterActionDescription>
        {
            new BoneSlash().ActionDescription,
            new RattleGuard().ActionDescription,
        };
        CharacterType = "Skeleton";
        Health = 20;
        Id = Guid.NewGuid().ToString();
        MaxHealth = 20;
        Name = "Skeleton";
    }
}
