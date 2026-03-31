using Models.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models.CharacterData;
public class Skeleton :Character
{
    public Skeleton()
    {
        Id = Guid.NewGuid().ToString();
        Name = "Skeleton";
        CharacterType = "Skeleton";
        Health = 20;
        MaxHealth = 20;
        ActionsDescriptions = [
            new RattleGuard().ActionDescription,
            new BoneSlash().ActionDescription,
            ];
    }
}
