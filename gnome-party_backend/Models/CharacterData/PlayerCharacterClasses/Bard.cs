using Models.Actions;
using Models.Actions.PlayerClassActions.BardActions;

namespace Models.CharacterData.PlayerCharacterClasses
{
    public sealed class Bard : Character
    {
        public string CurrentSong;
        public Bard() : this(Guid.NewGuid().ToString()) { }

        public Bard(string id) : base(id)
        {
            // List of actions availiable to the Bard
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new Discord().ActionDescription,
                new SoothingSong().ActionDescription,
                new Mockery().ActionDescription,
                new PowerCord().ActionDescription,
            };
            CharacterType = "Bard";
            Health = 25;
            MaxHealth = 25;
            Name = "Bard";
            CurrentSong = BardSongs.Soothing;
        }
        public CharacterActionDescription GetCurrentSong() 
        { 
            var possibleSongActionDescriptions = new List<CharacterActionDescription>
            {
                new SoothingSong().ActionDescription,
                new InspiringSong().ActionDescription,
                new FrighteningSong().ActionDescription,
            };
            foreach(var songActionDescription in possibleSongActionDescriptions)
            {
                if (ActionsDescriptions.Exists(ad => ad.Equals(songActionDescription)))
                {
                    return songActionDescription;
                }
            }
            return new SoothingSong().ActionDescription; // Default to Soothing Song if no match is found
        } 

        // List of bardic songs
        public static class BardSongs 
        {
            public const string Soothing = "Soothing Song";
            public const string Inspiring = "Inspiring Song";
            public const string Frightening = "Frightening Song";
        }

        public override Character DeepCopy()
        {
            return new Bard(Id)
            {
                Name = Name,
                CharacterType = CharacterType,
                Health = Health,
                MaxHealth = MaxHealth,
                CurrentSong = CurrentSong,
                ActionsDescriptions = new List<CharacterActionDescription>(ActionsDescriptions),
                StatusEffects = StatusEffects.Select(s => s.DeepCopy()).ToList(),
            };
        }
    }
}
