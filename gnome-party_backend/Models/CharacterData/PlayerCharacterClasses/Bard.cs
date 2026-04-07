using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Models.Actions.BardActions;

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
                new Discord().ActionDescription,
                new Song().ActionDescription,
                new Mockery().ActionDescription,
            };
            CharacterType = "Bard";
            Health = 25;
            MaxHealth = 25;
            Name = "Bard";
            CurrentSong = BardSongs.Soothing;
        }
        public Bard(string id) : base(id)
        {
            // List of actions availiable to the Bard
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new Discord().ActionDescription,
                new Song().ActionDescription,
                new Mockery().ActionDescription,
            };
            CharacterType = "Bard";
            Health = 25;
            MaxHealth = 25;
            Name = "Bard";
            CurrentSong = BardSongs.Soothing;
        }

        public void ChangeBardicSong(string currentSong)
        {
            // Switch statement to change the song that is being played after the Song Action was taken
            if(currentSong == null) { throw new ArgumentNullException(nameof(currentSong)); }
           
            switch(currentSong)
            {
                case BardSongs.Soothing:
                    CurrentSong = BardSongs.Inspiring;
                    break;
                case BardSongs.Inspiring:
                    CurrentSong = BardSongs.Frightening;
                    break;
                case BardSongs.Frightening:
                    CurrentSong = BardSongs.Soothing;
                    break;
                default:
                    CurrentSong = BardSongs.Soothing;
                    break;
            };
        }

        public string GetCurrentSong() { return CurrentSong; } // Accessible method to return the current bardic song

        // List of bardic songs
        public static class BardSongs 
        {
            public const string Soothing = "Soothing Song";
            public const string Inspiring = "Inspiring Song";
            public const string Frightening = "Frightening Song";
        }
    }
}
