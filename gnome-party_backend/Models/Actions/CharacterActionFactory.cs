using Models.Actions.BoosPoolActions.GnomeEaterActions;
using Models.Actions.BossPoolActions.NecrognomancerActions;
using Models.Actions.DifficultEnemyPoolActions.CaveBatActions;
using Models.Actions.DifficultEnemyPoolActions.GnombieBruteActions;
using Models.Actions.EasyEnemyPoolActions.ForestSpriteActions;
using Models.Actions.EasyEnemyPoolActions.GoblinArcherActions;
using Models.Actions.EasyEnemyPoolActions.SkeletonActions;
using Models.Actions.ExtraActions;
using Models.Actions.PlayerClassActions.BardActions;
using Models.Actions.PlayerClassActions.MageActions;
using Models.Actions.PlayerClassActions.MageUpgradeActions;
using Models.Actions.PlayerClassActions.WarriorActions;
using Models.Actions.PlayerClassActions.WarriorUpgradeActions;
using Models.TestHelperData;

namespace Models.Actions;

public class CharacterActionFactory
{
    // Class that creates CharacterAction instances based on action names.
    public static CharacterAction CreateCharacterAction(string actionName, IRandomGenerator? rng = null)
    {
        rng ??= new RandomNumGen();
        return actionName switch
        {
            // Warrior Attacks
            "Slash" => new Slash(),
            "Block" => new Block(),
            "Parry" => new Parry(),
            "Whirling Strike" => new WhirlingStrike(),

            // Warrior Upgrade Attacks
            "Precise Strike" => new PreciseStrike(),
            "Expert Guard" => new ExpertGuard(),
            "Counter" => new Counter(),
            "Devastating Cleave" => new DevastatingCleave(),

            // Mage Attacks
            "Magic Missile" => new MagicMisslie(),
            "Fireball" => new Fireball(),
            "Ice Ray" => new IceRay(),
            "Mirror" => new Mirror(),

            // Mage Upgrade Attacks
            "Vaporizing Beam" => new VaporizingBeam(),
            "Chain Lightning" => new ChainLightning(),
            "Blizzard" => new Blizzard(),

            // Bard Attacks
            "Discord" => new Discord(),
            "Mockery" => new Mockery(),
            "Song" => new Song(),
            "Soothing Song" => new SoothingSong(),
            "Inspiring Song" => new InspiringSong(),
            "Frightening Song" => new FrighteningSong(),
            "Power Chord" => new PowerCord(),

            // Skeleton Attacks
            "Bone Slash" => new BoneSlash(),
            "Rattle Guard" => new RattleGuard(),

            // Goblin Archer Attacks
            "Piercing Arrow" => new PiercingArrow(),
            "Crippling Shot" => new CripplingShot(),

            // Forest Sprite Attacks
            "Leaf Dart" => new LeafDart(),
            "Entangle" => new Entangle(),

            // Gnombie Brute Attacks
            "Heavy Slam" => new HeavySlam(),
            "Rotting Aura" => new RottingAura(),

            // Cave Bat Attacks
            "Sonic Squeal" => new SonicSqueal(),
            "Blood Peck" => new BloodPeck(),

            // Necrognomancer Attacks
            "Dark Bolt" => new DarkBolt(),
            "Soul Drain" => new SoulDrain(),
            "Summon" => new Summon(rng),

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
