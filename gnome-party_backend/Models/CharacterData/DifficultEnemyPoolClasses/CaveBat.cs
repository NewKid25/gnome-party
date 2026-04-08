using System;
using System.Collections.Generic;
using System.Text;

namespace Models.CharacterData.DifficultEnemyPoolClasses
{
    public class CaveBat : Character
    {
        public CaveBat()
        {
            ActionsDescriptions = new List<CharacterActionDescription>
            {

            };
            CharacterType = "Cave Bat";
            Health = 9;
            Id = Guid.NewGuid().ToString();
            MaxHealth = 9;
            Name = "Cave Bat";
        }
    }
}
