using System;
using System.Collections.Generic;
using System.Text;
using Models.Actions;
using Models.Actions.DifficultEnemyPoolActions.CaveBatActions;

namespace Models.CharacterData.DifficultEnemyPoolClasses
{
    public class CaveBat : Character
    {
        public CaveBat()
        {
            // List of actions available to the Cave Bat
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new SonicSqueal().ActionDescription,
                new BloodPeck().ActionDescription,
            };
            CharacterType = "Cave Bat";
            Health = 9;
            Id = Guid.NewGuid().ToString();
            MaxHealth = 9;
            Name = "Cave Bat";
        }
    }
}
