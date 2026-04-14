using System;
using System.Collections.Generic;
using System.Text;
using Models.Actions;
using Models.Actions.EasyEnemyPoolActions.ForestSpriteActions;

namespace Models.CharacterData.EasyEnemyPoolClasses
{
    public class ForestSprite : Character
    {
        public ForestSprite() 
        {
            ActionsDescriptions = new List<CharacterActionDescription> 
            {
                new LeafDart().ActionDescription,
                new Entangle().ActionDescription,
            };
            CharacterType = "Forest Sprite";
            Health = 12;
            Id = Guid.NewGuid().ToString();
            MaxHealth = 12;
            Name = "Forest Sprite";
        }
    }
}
