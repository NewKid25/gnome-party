using Models.CharacterData;
using System;
using System.Collections.Generic;
using System.Text;
using Models.AI;
using Models.CombatData;

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

    public CombatRequest ChooseAction(List<Character> enemies, List<Character> allies)
    {
        var actions = new List<string>();
        foreach (var actionDescription in Character.ActionsDescriptions)
        {
            actions.Add(actionDescription.Name);
        }
        var combatRequest = AI.ChooseAction(actions, enemies, allies);
        combatRequest.SourceCharacterId = Character.Id; //i do this here because AI doesnt have reference to source, and feels silly to pass one in
        return combatRequest;
    }
}
