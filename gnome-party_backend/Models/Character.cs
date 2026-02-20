namespace Models;

public class Character
{
    public string name { get; set; }
    public int health { get; set; }
    public Character()
    {
        name = "Default Name";
        health = 1;
    }
}
