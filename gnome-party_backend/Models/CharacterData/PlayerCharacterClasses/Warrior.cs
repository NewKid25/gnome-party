using Models.Actions;

namespace Models.CharacterData.PlayerCharacterClasses
{
    public sealed class Warrior : Character
    {
        public Warrior() : base(Guid.NewGuid().ToString())
        {
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new Block().ActionDescription,
                new Parry().ActionDescription,
                new Slash().ActionDescription,
                new WhirlingStrike().ActionDescription,
                
                //new FuryStrikes().ActionDescription,
            };
            CharacterType = "Warrior";
            Health = 30;
            MaxHealth = 30;
            Name = "Warrior";
        }
        public Warrior(string id) : base(id)
        {
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new Block().ActionDescription,
                new Parry().ActionDescription,
                new Slash().ActionDescription,
                new WhirlingStrike().ActionDescription,
                
                //new FuryStrikes().ActionDescription,

            };
            CharacterType = "Warrior";
            Health = 30;
            MaxHealth = 30;
            Name = "Warrior";
        }
    }
}
