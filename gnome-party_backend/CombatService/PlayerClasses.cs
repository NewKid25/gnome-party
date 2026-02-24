using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CombatService.Function;

namespace CombatService
{
    public enum PlayerCharacterClass
    {
        Warrior,
        Bard,
        Mage,
    }
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
