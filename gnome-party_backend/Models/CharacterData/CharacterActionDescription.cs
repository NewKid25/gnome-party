using System;
using System.Collections.Generic;
using System.Text;

namespace Models.CharacterData;
public class CharacterActionDescription
{
    public string Description { get; set; }
    public string Name { get; set; }

    public CharacterActionDescription() 
    {
        Name = "default_action_name";
        Description = "default_action_description";
    }
    public CharacterActionDescription(string name, string description= "default_action_description")
    {
        Name = name;
        Description = description;
    }
}
