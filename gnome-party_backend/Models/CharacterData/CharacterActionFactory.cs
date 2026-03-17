using System;
using System.Collections.Generic;
using System.Text;

namespace Models.CharacterData;

public class CharacterActionFactory
{
        public static CharacterAction CreateCharacterAction(string actionName)
        {
            switch (actionName)
            {
                case "Slash":
                    return new Slash();
                case "Block":
                    return new Block();
                default:
                    throw new ArgumentException($"Unknown action name: {actionName}");
            }
    }
}
