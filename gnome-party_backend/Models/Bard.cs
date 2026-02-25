using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Bard : Character_Base
    {
        public Bard(string name, Guid characterID)
        {
            CharacterName = name;
            MaxHealth = 25;
            Health = 25;
            CharacterID = characterID;
        }
    }
}
