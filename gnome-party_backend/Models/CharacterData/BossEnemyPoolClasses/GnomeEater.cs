using System;
using System.Collections.Generic;
using System.Text;
using Models.Actions;
using Models.Actions.BoosPoolActions.GnomeEaterActions;

namespace Models.CharacterData.BossEnemyPoolClasses
{
    public class GnomeEater : Character
    {
        public int PermaDamageBoost = 0;
        public int turnCounter = 0;
        public GnomeEater()
        {
            ActionsDescriptions = new List<CharacterActionDescription> 
            {
                new CrushingSwipe().ActionDescription,
                new DevourEssence().ActionDescription,
                new PrimalRoar().ActionDescription,
                new RavenousGrowth().ActionDescription,
            };
            CharacterType = "Gnome Eater";
            Health = 55;
            MaxHealth = 55;
            Name = "The Gnome Eater";
            PermaDamageBoost = 0;
            turnCounter = 0;
        }
    }
}
