
namespace Models.Actions;
// Class for describing character actions
public class CharacterActionDescription
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
}
