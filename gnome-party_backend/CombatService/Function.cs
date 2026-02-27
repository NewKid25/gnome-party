using Amazon.Lambda.Core;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Models;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace CombatService
{
    public class Function
    {
        public CombatResult CombatRequestHandler(CombatRequest request, ILambdaContext context)
        {
            if (request == null)
            {
                return null;
            }

            Character_Base attacker = CreateCharacter(request.Attacker);
            Character_Base target = CreateCharacter(request.Target);

            if (attacker == null || target == null)
            {
                return null;
            }

            CharacterAction attack = null;
            foreach (CharacterAction a in attacker.Attacks)
            {
                if (a.AttackName == request.Attack)
                {
                    attack = a;
                    break;
                }
            }

            if (attack == null)
            {
                return null;
            }

            attacker.ResolveAttack(attack, target);

            CombatResult result = new CombatResult();
            result.AttackerId = attacker.CharacterID;
            result.AttackerName = attacker.CharacterName;
            result.TargetId = target.CharacterID;
            result.TargetName = target.CharacterName;
            result.TargetHealth = target.Health;

            return result;
        }
        private Character_Base CreateCharacter(PlayerCharacterClass type)
        {
            Guid id = Guid.NewGuid();

            if (type == PlayerCharacterClass.Warrior)
            {
                return new Warrior("Warrior", id);
            }
            else if (type == PlayerCharacterClass.Bard)
            {
                return new Bard("Bard", id);
            }
            else if (type == PlayerCharacterClass.Mage)
            {
                return new Mage("Mage", id);
            }
            else
            {
                return null;
            }
        }
        public class CombatRequest
        {
            public PlayerCharacterClass Attacker { get; set; }
            public PlayerCharacterClass Target { get; set; }
            public string Attack { get; set; }
        }
        public class CombatResult
        {
            public Guid AttackerId { get; set; }
            public string AttackerName { get; set; }
            public Guid TargetId { get; set; }
            public string TargetName { get; set; }
            public int TargetHealth { get; set; }
        }
    }
}