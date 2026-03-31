using Models.Actions;

namespace Models.CharacterData.PlayerCharacterClasses
{
    public sealed class Mage : Character
    {
        public Mage() : base(Guid.NewGuid().ToString())
        {
            Name = "Mage";
            CharacterType = "Mage";
            Health = 20;
            MaxHealth = 20;
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new SpecialFireball().ActionDescription,
            };
        }
        public Mage(string id) : base(id)
        {
            Name = "Mage";
            CharacterType = "Mage";
            Health = 20;
            MaxHealth = 20;
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new SpecialFireball().ActionDescription,
            };
        }
    }
}
