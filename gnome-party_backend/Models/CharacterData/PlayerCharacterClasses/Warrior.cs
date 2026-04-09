using Models.Actions;
using Models.Actions.PlayerClassActions.WarriorActions;

namespace Models.CharacterData.PlayerCharacterClasses
{
    public sealed class Warrior : Character
    {
        public Warrior() : base(Guid.NewGuid().ToString())
        {
            // List of actions available to the Warrior
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
            // List of actions available to the Warrior
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new Slash().ActionDescription,
                new Block().ActionDescription,
                new Parry().ActionDescription,
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
