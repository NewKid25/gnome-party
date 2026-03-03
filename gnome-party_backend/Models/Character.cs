namespace GnomeParty.Models;
public class Character
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public List<CharacterActionDescription> ActionsDescriptions { get; set; }
    public Character() : this(Guid.NewGuid().ToString()) { }

    public Character(string id)
    {
        Id = id;
        Name = "Default Name";
        Health = 1;
        MaxHealth = Health;
        ActionsDescriptions = [];
        ActionsDescriptions.Add(new Slash().ActionDescription);
        ActionsDescriptions.Add(new Block().ActionDescription);
    }
}
