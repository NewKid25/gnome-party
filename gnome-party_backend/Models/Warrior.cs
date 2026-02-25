using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Warrior : Character_Base
    {
        public Warrior(string name, Guid characterID)
        {
            CharacterName = name;
            MaxHealth = 30;
            Health = 30;
            CharacterID = characterID;
            Attacks.Add(new Slash());
            Attacks.Add(new Block());
        }
    }
}
