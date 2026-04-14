using System;
using System.Collections.Generic;
using System.Text;
using Models.Actions;
using Models.Actions.DifficultEnemyPoolActions.CaveBatActions;
using Models.Actions.DifficultEnemyPoolActions.GnombieBruteActions;

namespace Models.CharacterData.DifficultEnemyPoolClasses
{
    public class GnombieBrute : Character
    {
        public GnombieBrute()
        {
            // List of actions available to the Cave Bat
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new HeavySlam().ActionDescription,
                new RottenAura().ActionDescription,
            };
            CharacterType = "Gnombie Brute";
            Health = 30;
            Id = Guid.NewGuid().ToString();
            MaxHealth = 30;
            Name = "Gnombie Brute";
        }
    }
}
