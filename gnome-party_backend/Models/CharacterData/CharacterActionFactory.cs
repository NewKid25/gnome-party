using Models.Actions;
using Models.Actions.BardActions;
using Models.Actions.MageActions;
using Models.Actions.SkeletonActions;
using Models.Actions.WarriorActions;

namespace Models.CharacterData;

public class CharacterActionFactory
{
    // Class that creates CharacterAction instances based on action names.
        public static CharacterAction CreateCharacterAction(string actionName)
        {
        return actionName switch
        {
            // Warrior Attacks
            "Block" => new Block(),
            "Parry" => new Parry(),
            "Slash" => new Slash(),
            "Whirling Strike" => new WhirlingStrike(),

            // Mage Attacks
            "Fireball" => new Fireball(),
            "Ice Ray" => new IceRay(),
            "Magic Missile" => new MagicMisslie(),
            "Mirror" => new Mirror(),
            
            // Bard Attacks
            "Mockery" => new Mockery(),
            "Discord" => new Discord(),

            // Skeleton Attacks
            "Bone Slash" => new BoneSlash(),
            "Rattle Guard" => new RattleGuard(),

            // Extra/Practice Implementation Moves
            "Fury Strikes" => new FuryStrikes(),
            _ => throw new ArgumentException($"Unknown action name: {actionName}"),
        };
    }
}
