using System;

namespace Models;

public class CharacterModel
{
	public string name { get; set; }
	public int health { get; set; }
    public CharacterModel()
	{
		name = "Default Name";
        health = 1;
	}
}
