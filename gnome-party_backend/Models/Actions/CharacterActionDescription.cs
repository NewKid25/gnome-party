
namespace Models.Actions;
// Class for describing character actions
public class CharacterActionDescription : IEquatable<CharacterActionDescription>
{
    public string Description { get; set; }
    public string Name { get; set; }
    public CharacterActionTargetRule TargetRule { get; set; }

    public CharacterActionDescription() 
    {
        Name = "default_action_name";
        Description = "default_action_description";
        TargetRule = CharacterActionTargetRule.Enemy;
    }
    public CharacterActionDescription(string name, string description= "default_action_description", CharacterActionTargetRule targetRule = CharacterActionTargetRule.Enemy)
    {
        Name = name;
        Description = description;
        TargetRule = targetRule;
    }

    public bool Equals(CharacterActionDescription? other)
    {
        if (other == null) return false;
        return Name == other.Name;
    }
}