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
            "Slash" => new Slash(),
            "Block" => new Block(),
            "Bone Slash" => new BoneSlash(),
            "Fireball" => new Fireball(),
            "Fury Strikes" => new FuryStrikes(),
            "Rattle Guard" => new RattleGuard(),
            _ => throw new ArgumentException($"Unknown action name: {actionName}"),
        };
    }
}
