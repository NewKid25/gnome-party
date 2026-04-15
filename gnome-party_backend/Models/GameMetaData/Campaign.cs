using Amazon.DynamoDBv2.DataModel;
using Models.CharacterData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.CombatData;
using Models.EncounterData;


namespace Models.GameMetaData;

public class Campaign
{
    [DynamoDBProperty(typeof(EncounterListConverter))]
    public List<Encounter> Encounters { get; set; }
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
                        new Skeleton()
                        {
                            Health = 10,
                            MaxHealth = 10,
                        },
                        new Skeleton()
                    ]
                },
                new CombatEncounter
                {
                    Enemies = [
                        new Skeleton()
                        {
                            Health = 30,
                            MaxHealth = 30,
                        }
                    ]
                }
            ],
            // medium pool
            [
                new CombatEncounter
                {
                    Enemies = [
                        new Skeleton()
                        {
                            Health = 15,
                            MaxHealth = 15,
                        }
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
