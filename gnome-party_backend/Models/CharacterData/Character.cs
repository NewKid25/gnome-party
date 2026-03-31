using Models.Actions;
using Models.Status;

namespace Models.CharacterData;
public class Character
{
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
        //Health = rnd.Next(1,21);
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
            StatusEffects = new List<StatusEffect>()
        };
        return copy;

    }
}
