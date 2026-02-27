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
