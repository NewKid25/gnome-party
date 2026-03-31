using Models.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models.CharacterData.EasyEnemyPoolClasses;
public class Skeleton :Character
{
    public Skeleton()
    {
        ActionsDescriptions = [
            new BoneSlash().ActionDescription,
            new RattleGuard().ActionDescription,
            ];
        CharacterType = "Skeleton";
        Health = 20;
        Id = Guid.NewGuid().ToString();
        MaxHealth = 20;
        Name = "Skeleton";
    }
}
