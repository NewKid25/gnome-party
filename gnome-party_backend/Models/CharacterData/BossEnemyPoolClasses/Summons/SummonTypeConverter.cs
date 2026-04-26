namespace Models.CharacterData.BossEnemyPoolClasses.Summons
{
    public static class SummonTypeExtensions
    {
        public static string ToCharacterTypeString(this SummonType type)
        {
            return type switch
            {
                SummonType.Skeleton => "Skeleton",
                SummonType.ForestSprite => "Forest Sprite",
                SummonType.GoblinArcher => "Goblin Archer",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}