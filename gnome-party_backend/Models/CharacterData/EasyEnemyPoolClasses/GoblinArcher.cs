using System;
using System.Collections.Generic;
using System.Text;
using Models.Actions;
using Models.Actions.EasyEnemyPoolActions.GoblinArcherActions;

namespace Models.CharacterData.EasyEnemyPoolClasses
{
    public class GoblinArcher : Character
    {
        public GoblinArcher()
        {
            // List of actions available to the Goblin Archer
            ActionsDescriptions = new List<CharacterActionDescription> 
            {
                new PiercingArrow().ActionDescription,
                new CripplingShot().ActionDescription,
            };
            CharacterType = "Goblin Archer";
            Health = 15;
            Id = Guid.NewGuid().ToString();
            MaxHealth = 15;
            Name = "Goblin Archer";
        }
    }
}
