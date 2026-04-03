using Models.Actions;

namespace Models.CharacterData.PlayerCharacterClasses
{
    public sealed class Mage : Character
    {
        public Mage() : base(Guid.NewGuid().ToString())
        {
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new Fireball().ActionDescription,
            };
            CharacterType = "Mage";
            Health = 20;
            MaxHealth = 20;
            Name = "Mage";
        }
        public Mage(string id) : base(id)
        {
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new Fireball().ActionDescription,
                new MagicMisslie().ActionDescription,
            };
            CharacterType = "Mage";
            Health = 20;
            MaxHealth = 20;
            Name = "Mage";
        }
    }
}
