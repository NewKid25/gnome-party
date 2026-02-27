namespace Models;

public class Character
{
    public string id { get; set; }
    public string name { get; set; }
    public int health { get; set; }
    public Character()
    {
        id = Guid.NewGuid().ToString();
        name = "Default Name";
        health = 1;
    }
}
