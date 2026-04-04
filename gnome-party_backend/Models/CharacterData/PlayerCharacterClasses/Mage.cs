using Models.Actions;

namespace Models.CharacterData.PlayerCharacterClasses
{
    public sealed class Mage : Character
    {
        public Mage() : base(Guid.NewGuid().ToString())
        {
            // List of actions available to the Mage
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new Fireball().ActionDescription,
                new MagicMisslie().ActionDescription,
                new IceRay().ActionDescription,
                new Mirror().ActionDescription,
            };
            CharacterType = "Mage";
            Health = 20;
            MaxHealth = 20;
            Name = "Mage";
        }
        public Mage(string id) : base(id)
        {
            // List of actions available to the Mage
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new Fireball().ActionDescription,
                new MagicMisslie().ActionDescription,
                new IceRay().ActionDescription,
                new Mirror().ActionDescription,
            };
            CharacterType = "Mage";
            Health = 20;
            MaxHealth = 20;
            Name = "Mage";
        }
    }
}
