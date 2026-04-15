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
    }
}
