using System.Text.Json.Serialization;
using Models.Actions;
using Models.Actions.PlayerClassActions.WarriorActions;
using Models.Status;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CharacterData.DifficultEnemyPoolClasses;
using Models.CharacterData.BossEnemyPoolClasses;
using Models.CharacterData.BossEnemyPoolClasses.Summons;
using Amazon.DynamoDBv2.DataModel;
using Models.Status;

namespace Models.CharacterData
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(Warrior), "Warrior")]
    [JsonDerivedType(typeof(Mage), "Mage")]
    [JsonDerivedType(typeof(Bard), "Bard")]
    [JsonDerivedType(typeof(Skeleton), "Skeleton")]
    [JsonDerivedType(typeof(GoblinArcher), "GoblinArcher")]
    [JsonDerivedType(typeof(ForestSprite), "ForestSprite")]
    [JsonDerivedType(typeof(GnombieBrute), "GnombieBrute")]
    [JsonDerivedType(typeof(CaveBat), "CaveBat")]
    [JsonDerivedType(typeof(GnomeEater), "GnomeEater")]
    [JsonDerivedType(typeof(Necrognomancer), "Necrognomancer")]
    [JsonDerivedType(typeof(Summons), "Summons")]
    public class Character
    {
        // Class for representing a character in the game
        public string CharacterType { get; set; }
        public int Health { get; set; }
        public string Id { get; set; }
        public int MaxHealth { get; set; }
        public string Name { get; set; }
        public Character() : this(Guid.NewGuid().ToString()) { }
        public Character(string id)
        {
            Id = id;
            Name = "Default Name";
            CharacterType = "Default Character Type";
            Health = 30;
            MaxHealth = Health;
            ActionsDescriptions = [];
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new Slash().ActionDescription,
                new Block().ActionDescription
            };
        }

        //ignore descriptions by default to avoid unnecessary data size during combat
        //to send descriptions to frontend, get it out seperately
        //[JsonIgnore] 
        public List<CharacterActionDescription> ActionsDescriptions { get; set; }

        [DynamoDBProperty(typeof(StatusEffectListConverter))]
        public List<StatusEffect> StatusEffects { get; set; } = new();
        public virtual Character DeepCopy()
        {
            var copy = new Character(Id)
            {
                Name = Name,
                CharacterType = CharacterType,
                Health = Health,
                MaxHealth = MaxHealth,
                ActionsDescriptions = new List<CharacterActionDescription>(ActionsDescriptions),
                StatusEffects = StatusEffects.Select(s => s.DeepCopy()).ToList(),
            };
            return copy;

        }
    }
}
