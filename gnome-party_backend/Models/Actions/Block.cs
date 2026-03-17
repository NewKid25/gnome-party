using GnomeParty.Models;
using Models.CharacterData;

namespace Models.Actions
{
    public sealed class Block : CharacterAction
    {
        public Block() : base("Block") { }
        public override void ApplyEffect(Character user, Character ally, AttackContext context)
        {
            int rounds = 2; // for tsting 2. but in practice 1
            double multiplier = 0.5;
        }
    }
}
