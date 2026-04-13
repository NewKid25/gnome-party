using Models.Actions.BoosPoolActions.GnomeEaterActions;
using Models.Actions.DifficultEnemyPoolActions.CaveBatActions;
using Models.Actions.EasyEnemyPoolActions.GoblinArcherActions;
using Models.Actions.EasyEnemyPoolActions.SkeletonActions;
using Models.Actions.ExtraActions;
using Models.Actions.PlayerClassActions.BardActions;
using Models.Actions.PlayerClassActions.MageActions;
using Models.Actions.PlayerClassActions.WarriorActions;

namespace Models.Actions;

public class CharacterActionFactory
{
    // Class that creates CharacterAction instances based on action names.
        public static CharacterAction CreateCharacterAction(string actionName)
        {
        return actionName switch
        {
            // Warrior Attacks
            "Slash" => new Slash(),
            "Block" => new Block(),
            "Parry" => new Parry(),
            "Whirling Strike" => new WhirlingStrike(),

            // Mage Attacks
            "Magic Missile" => new MagicMisslie(),
            "Fireball" => new Fireball(),
            "Ice Ray" => new IceRay(),
            "Mirror" => new Mirror(),

            // Bard Attacks
            "Discord" => new Discord(),
            "Mockery" => new Mockery(),
            "Song" => new Song(),
            "Soothing Song" => new SoothingSong(),
            "Inspiring Song" => new InspiringSong(),
            "Frightening Song" => new FrighteningSong(),
            "Power Cord" => new PowerCord(),

            // Skeleton Attacks
            "Bone Slash" => new BoneSlash(),
            "Rattle Guard" => new RattleGuard(),

            // Goblin Archer Attacks
            "Piercing Arrow" => new PiercingArrow(),
            "Crippling Shot" => new CripplingShot(),

            // Cave Bat Attacks
            "Sonic Squeal" => new SonicSqueal(),
            "Blood Peck" => new BloodPeck(),

            // Gnome Eater Attacks
            "Crushing Swipe" => new CrushingSwipe(),
            "Devour Essence" => new DevourEssence(),
            "Primal Roar" => new PrimalRoar(),
            "Ravenous Growth" => new RavenousGrowth(),

            // Extra/Practice Implementation Moves
            "Fury Strikes" => new FuryStrikes(),
            _ => throw new ArgumentException($"Unknown action name: {actionName}"),
        };
    }
}
