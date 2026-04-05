using System;
using System.Collections.Generic;
using System.Text;
using Models.Actions;

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
            

            // Skeleton Attacks
            "Bone Slash" => new BoneSlash(),
            "Rattle Guard" => new RattleGuard(),

            // Extra/Practice Implementation Moves
            "Fury Strikes" => new FuryStrikes(),
            _ => throw new ArgumentException($"Unknown action name: {actionName}"),
        };
    }
}
