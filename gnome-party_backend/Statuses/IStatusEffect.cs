namespace Statuses
{
    public interface IStatusEffect
    {
        string StatusName { get; }
        int RoundsRemaining { get; }
        bool IsExpired { get; }
        void OnRoundStart(Character_Base owner);
        void OnRoundEnd(Character_Base owner);
        void OnBeforeBeingAttacked(Character_Base owner, AttackContext context);
        void OnModifyIncomingDamage(Character_Base owner, AttackContext context);
        void ProgressRound();
    }
}