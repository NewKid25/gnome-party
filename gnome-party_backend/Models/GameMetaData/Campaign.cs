using Amazon.DynamoDBv2.DataModel;
using Models.CharacterData;
using Models.CharacterData.DifficultEnemyPoolClasses;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CombatData;
using Models.EncounterData;


namespace Models.GameMetaData;

public class Campaign
{
    [DynamoDBProperty(typeof(EncounterListConverter))]
    public List<Encounter> Encounters { get; set; }
    [DynamoDBProperty(typeof(CharacterListConverter))]
    public List<Character> PlayerCharacters { get; set; }
    public int CurrentEncounterIndex { get; set; } = 0;
    public Campaign()
    {
        PlayerCharacters = new List<Character>();
        Encounters = [];
    }
    public void InitEncounters()
    {
        //Change later: right now every pool will have the same number of encounters included, but eventually we may want to have different numbers of encounters from each pool
        int numberOfEncountersToIncludePerPool = 2;
        List<List<Encounter>> encounterPools = [
            // easy pool
            [
                new CombatEncounter
                {
                    Enemies = [
                        new GoblinArcher(),
                        new GoblinArcher(),
                    ]
                },
                new CombatEncounter
                {
                    Enemies = [
                        new Skeleton(),
                        new Skeleton(),
                    ]
                },
                new CombatEncounter
                {
                    Enemies = [
                        new Skeleton(),
                        new GoblinArcher(),
                    ]
                },
                new CombatEncounter
                {
                    Enemies = [
                        new GnombieBrute(),
                    ]
                },
                
                // Using less tested enemies
                new CombatEncounter
                {
                    Enemies = [
                        new GoblinArcher(),
                        new ForestSprite(),
                    ]
                },
                new CombatEncounter
                {
                    Enemies = [
                        new CaveBat(),
                        new Skeleton(),
                        new CaveBat(),
                    ]
                }

            ],
            // medium pool
            [
                new CombatEncounter
                {
                    Enemies = [
                        new Skeleton(),
                        new GnombieBrute(),
                        new Skeleton(),
                    ]
                },
                new CombatEncounter
                {
                    Enemies = [
                        new GnombieBrute(),
                        new GnombieBrute(),
                    ]
                },
                new CombatEncounter
                {
                    Enemies = [
                        new GoblinArcher(),
                        new Skeleton(),
                        new Skeleton(),
                        new GoblinArcher(),
                    ]
                },

                // Using less tested enemies
                new CombatEncounter
                {
                    Enemies = [
                        new GoblinArcher(),
                        new ForestSprite(),
                        new GoblinArcher(),
                    ]
                },
                new CombatEncounter
                {
                    Enemies = [
                        new CaveBat(),
                        new Skeleton()
                        {
                            Health = 40,
                            MaxHealth = 40,
                        },
                        new CaveBat(),
                    ]
                },
                new CombatEncounter
                {
                    Enemies = [
                        new CaveBat(),
                        new CaveBat(),
                        new CaveBat(),
                        new CaveBat(),
                    ]
                }
            ]
        ];

        foreach (var pool in encounterPools)
        {
            var radomOrderedPool = pool.Shuffle();
            Encounters.AddRange(radomOrderedPool.Take(numberOfEncountersToIncludePerPool));
        }
    }
}
