using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatService
{
    public enum EasyEnemyPoolClass 
    {
        Skeleton_Warrior,
        Goblin_Archer,
        Forest_Sprite
    }
    public class Skeleton_Warrior : Character_Base
    {
        private static readonly Random random = new Random();
        public Skeleton_Warrior(string name, Guid characterID)
        {
            CharacterName = name;
            MaxHealth = 20;
            Health = 20;
            CharacterID = characterID;
            Attacks.Add(new Bone_Slash());
            Attacks.Add(new Rattle_Guard());
        }
        protected override AttackAction ChooseAttack(List<Character_Base> aliveOpponents)
        {
            if (Attacks == null || Attacks.Count == 0) return null;

            double hpPercentaage = DetermineHpPercentage();
            double minHealthBeforeRattleGuard = 0.3;
            double chanceOfRattleGuard = 0.4;
            if (hpPercentaage <= minHealthBeforeRattleGuard)
            {
                double chance = random.NextDouble();
                if (chance <= chanceOfRattleGuard)
                {
                    for (int i = 0; i < Attacks.Count; i++)
                    {
                        if (Attacks[i] is Rattle_Guard)
                        {
                            return Attacks[i];
                        }
                    }
                }
            }
            else 
            { 
                for (int i = 0; i < Attacks.Count; i++)
                {
                    if (Attacks[i] is Bone_Slash)
                    {
                        return Attacks[i];
                    }
                }
            }
            return base.ChooseAttack(aliveOpponents);
        }
    }
    public class Goblin_Archer : Character_Base
    {
        public Goblin_Archer(string name, Guid characterID)
        {
            CharacterName = name;
            MaxHealth = 15;
            Health = 15;
            CharacterID = characterID;
        }
    }
    public class Forest_Sprite : Character_Base
    {
        public Forest_Sprite(string name, Guid characterID)
        {
            CharacterName = name;
            MaxHealth = 12;
            Health = 12;
            CharacterID = characterID;
        }
    }
}
