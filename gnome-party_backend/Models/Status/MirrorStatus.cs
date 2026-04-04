using System;
using System.Collections.Generic;
using System.Text;
using Models.CharacterData;

namespace Models.Status
{
    public sealed class MirrorStatus : StatusEffect
    {
        public MirrorStatus() { }
        public MirrorStatus(Character user, Character target)
        {
            Duration = 2; // Lasts until the start of the user's next turn
            DurationUnit = DurationUnit.TurnEnd; // Mirror is active until the start of the user's next turn;
            SourceCharacterId = user.Id; // The character who applied the mirror
            StatusOwnerCharacterId = user.Id; // The character who will mirror their attack onto the target
            AffectedCharacterIds = new List<string> { target.Id }; // The character will have an attack mirrored onto them
            StatusDescription = new Dictionary<string, string> // Text descriptions for the mirror status
            {
                ["AppliedText"] = $"{user.Name} activates mirror against {target.Name}.",
                ["ActiveText"] = $"{user.Name} will mirror their next attack onto {target.Name}.",
                ["ExpiredText"] = $"{user.Name}'s mirror effect has expired."
            };
        }
        public override StatusEffect DeepCopy() // Creates a deep copy of the MirrorStatus instance
        {
            return new MirrorStatus
            {
                AffectedCharacterIds = new List<string>(AffectedCharacterIds),
                Duration = Duration,
                DurationUnit = DurationUnit,
                SourceCharacterId = SourceCharacterId,
                StatusDescription = new Dictionary<string, string>(StatusDescription),
                StatusOwnerCharacterId = StatusOwnerCharacterId
            };
        }
    }
}
