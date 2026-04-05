using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Models.CharacterData.PlayerCharacterClasses
{
    public sealed class Bard : Character
    {
        public string CurrentSong;
        public Bard() : base(Guid.NewGuid().ToString())
        {
            // List of actions availiable to the Bard
            ActionsDescriptions = new List<CharacterActionDescription>
            {
            };
            CharacterType = "Bard";
            Health = 25;
            MaxHealth = 25;
            Name = "Bard";
            CurrentSong = "Soothing Song";
        }
        public Bard(string id) : base(id)
        {
            // List of actions availiable to the Bard
            ActionsDescriptions = new List<CharacterActionDescription>
            {
            };
            CharacterType = "Bard";
            Health = 25;
            MaxHealth = 25;
            Name = "Bard";
            CurrentSong = "Soothing Song";
        }
    }
}
