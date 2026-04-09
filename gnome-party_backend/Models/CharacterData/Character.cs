using Models.Actions;
using Models.Actions.PlayerClassActions.WarriorActions;
using Models.Status;

namespace Models.CharacterData;
public class Character
{
    // Class for representing a character in the game
    public string CharacterType { get; set; }
    public int Health { get; set; }
    public string Id { get; set; }
    public int MaxHealth { get; set; }
    public string Name { get; set; }
    public Character() : this(Guid.NewGuid().ToString()) { }
    public Character(string id)
    {
        Random rnd = new Random();
        Id = id;
        Name = "Default Name";
        CharacterType = "Default Character Type";
        Health = 30;
        MaxHealth = Health;
        ActionsDescriptions = [];
        ActionsDescriptions.Add(new Slash().ActionDescription);
        ActionsDescriptions.Add(new Block().ActionDescription);
    }
    public List<CharacterActionDescription> ActionsDescriptions { get; set; }
    public List<StatusEffect> StatusEffects { get; set; } = new();
    public Character DeepCopy()
    {
        var copy = new Character(Id)
        {
            Name = Name,
            CharacterType = CharacterType,
            Health = Health,
            MaxHealth = MaxHealth,
            ActionsDescriptions = new List<CharacterActionDescription>(ActionsDescriptions),
            StatusEffects = StatusEffects.Select(s => s.DeepCopy()).ToList(),
        };
        return copy;

    }
}
