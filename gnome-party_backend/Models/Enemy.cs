using Models.CharacterData;
using System;
using System.Collections.Generic;
using System.Text;
using Models.AI;
using Models.CombatData;
using Models.CharacterData.EasyEnemyPoolClasses;
using Models.AI.EasyEnemyPoolAI;
using Models.AI.DifficultEnemyPoolAI;
using Models.CharacterData.DifficultEnemyPoolClasses;
using Models.TestHelperData;

namespace Models;
public class Enemy
{
    Character Character { get; set; }
    CharacterAI AI { get; set; }
    public Enemy(Character character) : this(character.CharacterType, character, new RandomNumGen()) { }
    public Enemy(Character character, IRandomGenerator rng) : this(character.CharacterType, character, rng) { }
    public Enemy(string characterType, Character? character = null, IRandomGenerator? rng = null)
    {
        if (characterType == null)  { throw new ArgumentNullException(nameof(characterType)); }

        rng ??= new RandomNumGen(); // threw in a random number generator functionality for testing

        switch (characterType)
        {
            case "Skeleton":
                Character = character ??  new Skeleton();
                AI = new SkeletonAI(rng);
                break;
            case "Cave Bat":
                Character = character ?? new CaveBat();
                AI = new CaveBatAI(rng);
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
        var combatRequest = AI.ChooseAction(Character, actions, enemies, allies);
        combatRequest.SourceCharacterId = Character.Id; //i do this here because AI doesnt have reference to source, and feels silly to pass one in
        return combatRequest;
    }
}
