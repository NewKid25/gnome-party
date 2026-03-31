using System;
using System.Collections.Generic;
using System.Text;
using Models.Actions;

namespace Models.CharacterData;

public class CharacterActionFactory
{
        public static CharacterAction CreateCharacterAction(string actionName)
        {
        return actionName switch
        {
            // Warrior Attacks
            "Block" => new Block(),
            "Parry" => new Parry(),
            "Slash" => new Slash(),
            "Whirling Strike" => new WhirlingStrike(),

            // Skeleton Attacks
            "Bone Slash" => new BoneSlash(),
            "Rattle Guard" => new RattleGuard(),

            // Extra/Practice Implementation Moves
            "Special Fireball" => new Fireball(),
            "Fury Strikes" => new FuryStrikes(),
            _ => throw new ArgumentException($"Unknown action name: {actionName}"),
        };
    }
}
