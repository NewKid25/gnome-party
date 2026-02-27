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
