using System;
using System.Collections.Generic;
using System.Text;
using Models.Actions;

namespace Models.CharacterData
{
    public sealed class Warrior : Character
    {
        public Warrior() : base(Guid.NewGuid().ToString())
        {
            Name = "Warrior";
            CharacterType = "Warrior";
            Health = 30;
            MaxHealth = 30;
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new Block().ActionDescription,
                new Slash().ActionDescription,
                new WhirlingStrike().ActionDescription,
                new Parry().ActionDescription,
                
                //new FuryStrikes().ActionDescription,
            };
        }
        public Warrior(string id) : base(id)
        {
            Name = "Warrior";
            CharacterType = "Warrior";
            Health = 30;
            MaxHealth = 30;
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new Block().ActionDescription,
                new Slash().ActionDescription,
                new WhirlingStrike().ActionDescription,
                new Parry().ActionDescription,
                
                //new FuryStrikes().ActionDescription,

            };
        }
    }
}
