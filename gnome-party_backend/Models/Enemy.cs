using Models.CharacterData;
using System;
using System.Collections.Generic;
using System.Text;
using Models.AI;

namespace Models;

public class Enemy
{
    Character Character { get; set; }
    CharacterAI AI { get; set; }

    public Enemy(Character character) : this(character.CharacterType, character) { }

    public Enemy(string characterType, Character? character = null)
    { 
        switch (characterType)
        {
            case "Skeleton":
                Character = character ?? new Skeleton();
                AI = new SkeletonAI();
                break;
            default:
                throw new ArgumentException($"Unknown character type: {characterType}");
        }
    }
}
