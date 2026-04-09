using System;
using System.Collections.Generic;
using System.Text;
using Models.Actions;

namespace Models.CharacterData.BossEnemyPoolClasses
{
    public class GnomeEater : Character
    {
        public int PermaDamageBoost = 0;
        public GnomeEater()
        {
            ActionsDescriptions = new List<CharacterActionDescription> 
            {
            };
            CharacterType = "Gnome Eater";
            Health = 55;
            MaxHealth = 55;
            Name = "The Gnome Eater";
            PermaDamageBoost = 0;
        }
    }
}
