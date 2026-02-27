using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CombatService.Function;

namespace Models
{
    public enum PlayerCharacterClass
    {
        Warrior,
        Bard,
        Mage,
    }

    public class Mage : Character_Base
    {
        public Mage(string name, Guid characterID)
        {
            CharacterName = name;
            MaxHealth = 20;
            Health = 20;
            CharacterID = characterID;
        }
    }
}
