using System;
using System.Collections.Generic;
using System.Text;
using Models.Actions;
using Models.Actions.BossPoolActions.NecrognomancerActions;

namespace Models.CharacterData.BossEnemyPoolClasses
{
    public class Necrognomancer : Character
    {
        public Necrognomancer()
        {
            ActionsDescriptions = new List<CharacterActionDescription>
            {
                new DarkBolt().ActionDescription,
                new SoulDrain().ActionDescription,
            };
            CharacterType = "Necrognomancer";
            Health = 40;
            MaxHealth = 40;
            Name = "Necrognomancer";
        }
    }
}
