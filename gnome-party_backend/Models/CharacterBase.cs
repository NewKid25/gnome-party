using Statuses;

namespace Models
{
    public class Character_Base
    {
        public Guid CharacterID { get; set; }
        public string CharacterName { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public List<CharacterAction> Attacks { get; set; }
        public List<Character_Base> Opponents { get; set; }
        private readonly List<IStatusEffect> _statuses;
        private static readonly Random _random = new Random();
        public Character_Base()
        {
            CharacterName = string.Empty;
            Attacks = new List<CharacterAction>();
            Opponents = new List<Character_Base>();
            _statuses = new List<IStatusEffect>();
        }
        public bool IsAlive
        {
            get { return Health > 0; }
        }
        public IReadOnlyList<IStatusEffect> Statuses
        {
            get { return _statuses; }
        }
        public void AddStatus(IStatusEffect status)
        {
            if (status == null) { return; }
            _statuses.Add(status);
        }
        public void RemoveStatus(IStatusEffect status)
        {
            if (status == null) { return; }
            _statuses.Remove(status);
        }
        public void OnRoundStart()
        {
            List<IStatusEffect> preprocess = new List<IStatusEffect>(_statuses);
            for (int i = 0; i < preprocess.Count; i++)
            {
                preprocess[i].OnRoundStart(this);
            }
            CleanupExpiredStatuses();
        }
        public void OnRoundEnd()
        {
            List<IStatusEffect> postprocess = new List<IStatusEffect>(_statuses);
            for (int i = 0; i < postprocess.Count; i++)
            {
                postprocess[i].OnRoundEnd(this);
            }
            postprocess = new List<IStatusEffect>(_statuses);
            for (int i = 0; i < postprocess.Count; i++)
            {
                postprocess[i].ProgressRound();
            }
            CleanupExpiredStatuses();
        }
        private void CleanupExpiredStatuses()
        {
            for (int i = _statuses.Count - 1; i >= 0; i--) { if (_statuses[i].IsExpired) { _statuses.RemoveAt(i); } }
        }
        public List<Character_Base> GetAliveOpponents()
        {
            List<Character_Base> aliveOpponents = new List<Character_Base>();
            if (Opponents == null) { return aliveOpponents; }
            for (int i = 0; i < Opponents.Count; i++)
            {
                Character_Base o = Opponents[i];
                if (o != null && o.IsAlive)
                {
                    aliveOpponents.Add(o);
                }
            }
            return aliveOpponents;
        }
        protected virtual CharacterAction ChooseAttack(List<Character_Base> aliveOpponents)
        {
            if (Attacks == null || Attacks.Count == 0) { return null; }
            return Attacks[0];
        }
        protected virtual Character_Base ChooseTarget(List<Character_Base> aliveOpponents, CharacterAction chosenAttack)
        {
            if (aliveOpponents == null || aliveOpponents.Count == 0) { return null; }
            int index = _random.Next(0, aliveOpponents.Count);
            return aliveOpponents[index];
        }
        public void ResolvePlannedAction(PlannedAction plannedAction)
        {
            if (plannedAction == null) { return; }
            if (!IsAlive) { return; }
            CharacterAction attack = plannedAction.Attack;
            if (attack == null) { return; }
            if (plannedAction.GroupTargets != null && plannedAction.GroupTargets.Count > 0)
            {
                ResolveAttack(attack, plannedAction.GroupTargets, plannedAction.DuplicateTargets); 
                return;
            }
            Character_Base target = plannedAction.Target;
            if(target == null || !target.IsAlive)
            {
                List<Character_Base> aliveOpponents = GetAliveOpponents();
                target = ChooseTarget(aliveOpponents, attack);
            }
            if (target == null || !target.IsAlive) return;
            ResolveAttack(attack, target);
        }
        public void ResolveAttack(CharacterAction attackAction, Character_Base target)
        {
            List<Character_Base> opponentList = new List<Character_Base>();
            opponentList.Add(target);
            ResolveAttack(attackAction, opponentList, false);
        }
        public void ResolveAttack(CharacterAction attackAction, List<Character_Base> groupTargets, bool uniqueTargetsOnly)
        {
            if (attackAction == null) { return; }
            if (groupTargets == null || groupTargets.Count == 0) { return; }
            List<AttackContext> contexts = new List<AttackContext>();
            for (int i = 0; i < groupTargets.Count; i++)
            {
                Character_Base target = groupTargets[i];
                if (target == null || !target.IsAlive) { continue; }
                AttackContext cont = new AttackContext(this, attackAction, target, i);
                target.TriggerBeforeBeingAttacked(cont);
                if (cont.CurrentTarget != null && cont.CurrentTarget.IsAlive)
                {
                    contexts.Add(cont);
                }
            }
            if (uniqueTargetsOnly)
            {
                List<AttackContext> unique = new List<AttackContext>();
                HashSet<Guid> seen = new HashSet<Guid>();
                for (int i = 0; i < contexts.Count; i++)
                {
                    AttackContext cont = contexts[i];
                    if (cont.CurrentTarget == null) { continue; }
                    Guid id = cont.CurrentTarget.CharacterID;
                    if (!seen.Contains(id))
                    {
                        seen.Add(id);
                        unique.Add(cont);
                    }
                }
                contexts = unique;
            }
            for (int i = 0; i < contexts.Count; i++)
            {
                AttackContext cont = contexts[i];
                if (cont.CurrentTarget == null || !cont.CurrentTarget.IsAlive) { continue; }
                attackAction.ApplyEffect(this, cont.CurrentTarget, cont);
            }
        }
        private void TriggerBeforeBeingAttacked(AttackContext context)
        {
            List<IStatusEffect> applied = new List<IStatusEffect>(_statuses);
            for(int i = 0; i < applied.Count; i++)
            {
                applied[i].OnBeforeBeingAttacked(this, context);
            }
        }
        public void ReceiveDamage(int damage, AttackContext context)
        {
            if(context == null) {  return; }
            context.BaseDamage = damage;
            context.ModifiedDamage = damage;
            List<IStatusEffect> applieed = new List<IStatusEffect>(_statuses);
            for (int i = 0;i < applieed.Count;i++)
            {
                applieed[i].OnModifyIncomingDamage(this, context);
            }
            int finalDamage = Math.Max(0, context.ModifiedDamage);
            Health = Health - finalDamage;
            if (Health < 0) { Health = 0; }
        }
        public void ReceiveHealth(int healthReceived)
        {
            Health = Health + healthReceived;
            if (Health > MaxHealth) { Health = MaxHealth; }
            if (Health < 0) { Health = 0; }
        }
        public double DetermineHpPercentage()
        {
            if (MaxHealth <= 0) { return 0; }
            return (double)Health / (double)MaxHealth;
           
        }
        public virtual PlannedAction ChooseEnemyAction(List<Character_Base> playerParty, List<Character_Base> enemyGroup, Random random)
        {
            if (Attacks == null || Attacks.Count == 0) return null;
            List<Character_Base> aliverPlayers = new List<Character_Base>();
            for (int i = 0; i < playerParty.Count; i++)
            {
                Character_Base p = playerParty[i];
                if(p != null && p.IsAlive)
                {
                    aliverPlayers.Add(p);
                }
            }
            if (aliverPlayers.Count == 0) return null;
            if(random == null) random = new Random();
            Character_Base target = aliverPlayers[random.Next(0, aliverPlayers.Count)];
            PlannedAction action = new PlannedAction();
            action.User = this;
            action.Attack = Attacks[0];
            action.Target = target;
            action.GroupTargets = null;
            action.DuplicateTargets = false;
            return action;
        }
    }
}