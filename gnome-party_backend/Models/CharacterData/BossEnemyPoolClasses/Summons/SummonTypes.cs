namespace Models.CharacterData.BossEnemyPoolClasses.Summons
{
    public enum SummonType
    {
        Skeleton,
        ForestSprite,
        GoblinArcher
    }

    public static class SummonTypeData
    {
        public static readonly (SummonType Type, double Weight)[] WeightedSummons =
        {
            (SummonType.Skeleton, 0.5),
            (SummonType.ForestSprite, 0.3),
            (SummonType.GoblinArcher, 0.2),
        };
    }
}
