import Konva from "konva";
import { Tween } from "konva/lib/Tween";
import AnimationSequence from "./AnimationSequence";
import SimultaneousAnimation from "./SimultaneousAnimation";
import TweenFromCurrent from "./TweenFromCurrent";
import LeapAnimation from "./animations/LeapAnimation";
import AnimationPause from "./AnimationPause";
import GnomePuppet from "./puppets/GnomePuppet";
import HealthBar from "./HealthBar";
import FunctionStep from "./FunctionStep";
import AnimationStep from "./interfaces/AnimationStep";
import { Character, GameState, TurnStep } from "./interfaces/TurnStep";
import Puppet from "./interfaces/Puppet";
import SlashAnimation from "./action-animations/SlashAnimation";
import BoneSlashAnimation from "./action-animations/BoneSlashAnimation";
import { useEncounterData } from "../../participant-view/stores/encounterData";
import SkeletonPuppet from "./puppets/SkeletonPuppet";
import GoblinArcherPuppet from "./puppets/GoblinArcherPuppet";
import { C } from "vue-router/dist/options-CjwwR_07.cjs";
import ForestSpritePuppet from "./puppets/ForestSpritePuppet";
import CaveBatPuppet from "./puppets/CaveBatPuppet";
import GnombieBrutePuppet from "./puppets/GnombieBrutePuppet";
import GnomeEaterPuppet from "./puppets/GnomeEaterPuppet";
import NecrognomancerPuppet from "./puppets/NecrognomancerPuppet";
import TextPopupAnimation from "./animations/TextPopupAnimation";
import HeavySlamAnimation from "./action-animations/HeavySlamAnimation";
import CrushingSwipeAnimation from "./action-animations/CrushingSwipeAnimation";
import BlockAnimation from "./action-animations/BlockAnimation";
import ParryAnimation from "./action-animations/ParryAnimation";
import RattleGuardAnimation from "./action-animations/RattleGuardAnimation";
import WhirlingStrikeAnimation from "./action-animations/WhirlingStrikeAnimation";
import SongAnimation from "./action-animations/SongAnimation";
import DiscordAnimation from "./action-animations/DiscordAnimation";
import MockeryAnimation from "./action-animations/MockeryAnimation";
import IceRayAnimation from "./action-animations/IceRayAnimation";
import FireballAnimation from "./action-animations/FireballAnimation";
import MagicMissileAnimation from "./action-animations/MagicMissileAnimation";
import PiercingArrowAnimation from "./action-animations/PiercingArrowAnimation";
import CripplingShotAnimation from "./action-animations/CripplingShotAnimation";
import RottingAuraAnimation from "./action-animations/RottingAuraAnimation";
import DarkBoltAnimation from "./action-animations/DarkBoltAnimation";
import LeafDartAnimation from "./action-animations/LeafDartAnimation";
import DevourEssenceAnimation from "./action-animations/DevourEssenceAnimation";
import SonicSquealAnimation from "./action-animations/SonicSquealAnimation";
import BloodPeckAnimation from "./action-animations/BloodPeckAnimation";
import PrimalRoarAnimation from "./action-animations/PrimalRoarAnimation";
import SoulDrainAnimation from "./action-animations/SoulDrainAnimation";
import MirrorAnimation from "./action-animations/MirrorAnimation";
import RavenousGrowthAnimation from "./action-animations/RavenousGrowthAnimation";
import EntangleAnimation from "./action-animations/EntangleAnimation";
import PowerChordAnimation from "./action-animations/PowerChordAnimation";
import SummonAnimation from "./action-animations/SummonAnimation";

export default
class ViewManager {

	HEALTHBAR_HEIGHT = 120;


	socket:WebSocket

	stage:Konva.Stage

	mainLayer:Konva.Layer
	uiLayer:Konva.Layer
	
	playerVisualComponents:Map<string, CharacterVisualComponents> = new Map<string, CharacterVisualComponents>()
	enemyVisualComponents:Map<string, CharacterVisualComponents> = new Map<string, CharacterVisualComponents>()

	encounterData = useEncounterData();

	// track participants in lobby on host side
	readyPlayers:any[] = [];

	constructor() {
		this.socket = new WebSocket("wss://ws.gnome-party.com");

		this.socket.addEventListener("message", (ev) => {
			console.log("Message from server ", ev.data);
  			let parsedJSON = JSON.parse(ev.data);
			
			this.handleMessage(parsedJSON);
		});

		const container:HTMLDivElement = document.getElementById("konva-container") as HTMLDivElement;
	
		// first we need to create a stage
		this.stage = new Konva.Stage({
			container: 'konva-container', // id of container <div>
			width: container.clientWidth,
			height: container.clientHeight
		});
	
		// then create layers
		this.mainLayer = new Konva.Layer();
		this.uiLayer = new Konva.Layer();
	
		// add layers to the stage
		this.stage.add(this.mainLayer);
		this.stage.add(this.uiLayer);

		this.socket.onopen = (ev:Event) => {
			this.socket.send(JSON.stringify({route: "host-game"}))
		};
	}
	
	loadEncounter(gameState:any)
	{
		// Load player characters
		var playerCharacters = gameState.PlayerCharacters;

		for (let i = 0; i < playerCharacters.length; i++) {
			// Create GnomePuppet
			let puppet:GnomePuppet = new GnomePuppet();
			puppet.x(300);
			puppet.y((i + 1) * this.stage.height() / (playerCharacters.length + 1));

			this.mainLayer.add(puppet);
			// Create healthbar
			let healthbar:HealthBar = new HealthBar(playerCharacters[i].MaxHealth, {x: 30, y: this.HEALTHBAR_HEIGHT})
			healthbar.x(puppet.x() - puppet.width() /2 - 50);
			healthbar.y(puppet.y() - puppet.height() / 3.5);

			this.uiLayer.add(healthbar);

			this.playerVisualComponents.set(playerCharacters[i].Id, {puppet: puppet, healthbar: healthbar});
		}

		// Load enemy characters
		var enemyCharacters = gameState.EnemyCharacters;

		for (let i = 0; i < enemyCharacters.length; i++) {
			// Create puppet of corresponding enemy
			let puppet:Puppet
			let type:string = enemyCharacters[i].CharacterType;
			console.log("This guy is a", type);
			
			puppet = this.createEnemyPuppet(type);
			
			puppet.x(this.stage.width() - 300);
			puppet.y((i + 1) * this.stage.height() / (enemyCharacters.length + 1));

			this.mainLayer.add(puppet);
			// Create healthbar
			let healthbar:HealthBar = new HealthBar(enemyCharacters[i].MaxHealth, {x: 30, y: this.HEALTHBAR_HEIGHT})
			healthbar.x(puppet.x() + puppet.width() /2 + 20);
			healthbar.y(puppet.y() - puppet.height() / 3.5);

			this.uiLayer.add(healthbar);

			this.enemyVisualComponents.set(enemyCharacters[i].Id, {puppet: puppet, healthbar: healthbar});
		}
	}

	handleMessage(msg:any) {
		if (msg.Subject == "host-game")
		{
			this.encounterData.gameSessionId = msg.Message.GameSessionId;
			this.encounterData.localPlayerId = msg.Message.Host.UserId;

			// for testing
			console.log("Host game created");
			console.log("Invite code:", msg.Message.InviteCode);
			console.log("Game session:", msg.Message.GameSessionId);
		}
		if (msg.Subject === "lobby-ready") {
			this.readyPlayers.push(msg.Message);
			console.log("Participant readied:", msg.Message);
			console.log("Ready player count:", this.readyPlayers.length);
		}
		if (msg.Subject === "lobby-unready") {
			console.log("Participant unreadied:", msg.Message);
		}

		if (msg.Subject === "player-disconnected") {
			console.log("Participant disconnected:", msg.Message);
		}

		if (msg.Subject === "host-disconnected") {
			console.log("Host disconnected:", msg.Message);
		}

		if (msg.Subject === "start-campaign") {
			console.log("Campaign started:", msg.Message);
			this.socket.send(JSON.stringify({
				route: "begin-combat-encounter",
				GameSessionId: this.encounterData.gameSessionId
			}))
		}

		if (msg.Subject == "begin-combat-encounter")
		{
			this.encounterData.encounterId = msg.Message.EncounterId;
			this.loadEncounter(msg.Message.GameState);
		}
		if (msg.Subject == "action-handler")
		{
			this.processTurn(msg.Message);
		}
	}

	processTurn(turn:TurnStep[])
	{
		let animations:AnimationStep[] = [];
		// let finalStep:TurnStep|undefined;
		for (let step of turn)
		{
			let animation:AnimationStep | undefined = this.instantiateActionAnimation(step);
			if (animation) animations.push(animation);
			// finalStep = step;
		}

		if (turn.at(-1) != undefined)
			animations.push(new AnimationPause(500));
			animations.push(this.updateAllHealth(turn.at(-1).GameState))

		let sequence:AnimationSequence = new AnimationSequence(animations);
		sequence.play();
	}

	testAnimation()
	{
		// let anims:AnimationStep[] = []
		// for (const pair of this.playerVisualComponents) {
		// 	const pvc:CharacterVisualComponents = pair[1];

		// 	const enemy:CharacterVisualComponents = (this.enemyVisualComponents.get('0') as CharacterVisualComponents)

		// 	let anim:LeapAnimation = new LeapAnimation({
		// 		leapingNode: pvc.puppet, 
		// 		destination: enemy.puppet, 
		// 		landingAnimation: new FunctionStep(() => { enemy.healthbar.changeHealth(20 - (parseInt(pair[0]) + 1) * 2); })
		// 	});

		// 	anims.push(anim);
		// }

		// let sequence:AnimationSequence = new AnimationSequence(anims);

		// sequence.play();

		
		let gameState:GameState = {
			"EnemyCharacters": [
				{
				"CharacterType": "Necrognomancer",
				"Health": 55,
				"Id": "n",
				"MaxHealth": 55,
				"Name": "Forest Sprite",
				"ActionsDescriptions": [
					{
					"Description": "Deal 6 damage to target enemy",
					"Name": "Bone Slash"
					},
					{
					"Description": "Reduce damage by 50% for one turn",
					"Name": "Rattle Guard"
					}
				],
				"StatusEffects": []
				},
			],
			"PlayerCharacters": [
				{
				"CharacterType": "Mage",
				"Health": 20,
				"Id": "e246199a-3baa-4464-9e9b-6424d8054839",
				"MaxHealth": 20,
				"Name": "Mage",
				"ActionsDescriptions": [
					{
					"Description": "Deal damage to the target and then burn the target and adjacent allies for 3 turns",
					"Name": "Fireball"
					},
					{
					"Description": "Deal 10 damage to target enemy uninterrupted",
					"Name": "Magic Missile"
					},
					{
					"Description": "Deal 5 damage to a target and reduce their attack power.",
					"Name": "Ice Ray"
					},
					{
					"Description": "Target an enemy. Your next attack will also hit that enemy.",
					"Name": "Mirror"
					}
				],
				"StatusEffects": []
				},
			]
		};
		
		this.loadEncounter(gameState);

		let sampleSteps:TurnStep[] = 
		[
			{
			"GameState": {
				"EnemyCharacters": [
					{
					"CharacterType": "Necrognomancer",
					"Health": 55,
					"Id": "n",
					"MaxHealth": 55,
					"Name": "Forest Sprite",
					"ActionsDescriptions": [
						{
						"Description": "Deal 6 damage to target enemy",
						"Name": "Bone Slash"
						},
						{
						"Description": "Reduce damage by 50% for one turn",
						"Name": "Rattle Guard"
						}
					],
					"StatusEffects": []
					},
					{
					"CharacterType": "Skeleton",
					"Health": 10,
					"Id": "s1",
					"MaxHealth": 10,
					"Name": "Summoned Skeleton 1",
					"ActionsDescriptions": [
					{
						"Description": "Deal 6 damage to target enemy",
						"Name": "Bone Slash"
					},
					{
						"Description": "Reduce damage by 50% for one turn",
						"Name": "Rattle Guard"
					}
					],
					"StatusEffects": []
					},
					{
					"CharacterType": "Skeleton",
					"Health": 10,
					"Id": "s2",
					"MaxHealth": 10,
					"Name": "Summoned Skeleton 2",
					"ActionsDescriptions": [
					{
						"Description": "Deal 6 damage to target enemy",
						"Name": "Bone Slash"
					},
					{
						"Description": "Reduce damage by 50% for one turn",
						"Name": "Rattle Guard"
					}
					],
					"StatusEffects": []
					},
					{
					"CharacterType": "Skeleton",
					"Health": 10,
					"Id": "s3",
					"MaxHealth": 10,
					"Name": "Summoned Skeleton 3",
					"ActionsDescriptions": [
					{
						"Description": "Deal 6 damage to target enemy",
						"Name": "Bone Slash"
					},
					{
						"Description": "Reduce damage by 50% for one turn",
						"Name": "Rattle Guard"
					}
					],
					"StatusEffects": []
					}
				],
				"PlayerCharacters": [
					{
					"CharacterType": "Mage",
					"Health": 20,
					"Id": "e246199a-3baa-4464-9e9b-6424d8054839",
					"MaxHealth": 20,
					"Name": "Mage",
					"ActionsDescriptions": [
						{
						"Description": "Deal damage to the target and then burn the target and adjacent allies for 3 turns",
						"Name": "Fireball"
						},
						{
						"Description": "Deal 10 damage to target enemy uninterrupted",
						"Name": "Magic Missile"
						},
						{
						"Description": "Deal 5 damage to a target and reduce their attack power.",
						"Name": "Ice Ray"
						},
						{
						"Description": "Target an enemy. Your next attack will also hit that enemy.",
						"Name": "Mirror"
						}
					],
					"StatusEffects": []
					},
				]
			},
			"Request": {
				"Action": "Summon",
				"EncounterId": "93e31b11-5a36-45cb-82a1-b2a8c234f00a",
				"GameSessionId": "game1",
				"SourceCharacterId": "n",
				"TargetCharacterId": "warrior2"
			},
			"Events": [
				{
					"event": "summoned",
					"params": {
						"SourceId": "Necrognomancer",
						"SummonId": "s1",
						"SummonType": "Skeleton",
						"SummonName": "Summoned Skeleton 1"
					}
				},
				{
					"event": "summoned",
					"params": {
						"SourceId": "Necrognomancer",
						"SummonId": "s2",
						"SummonType": "Skeleton",
						"SummonName": "Summoned Skeleton 1"
					}
				},
				{
					"event": "summoned",
					"params": {
						"SourceId": "Necrognomancer",
						"SummonId": "s3",
						"SummonType": "Skeleton",
						"SummonName": "Summoned Skeleton 1"
					}
				},
			]
			},
		];

		this.processTurn(sampleSteps);
	}

	instantiateActionAnimation(step:TurnStep)
	{
		let actionName = step.Request.Action;
		switch(actionName) {
			case "Slash":
				return new SlashAnimation(step, this);
			case "Bone Slash":
				return new BoneSlashAnimation(step, this);
			case "Heavy Slam":
				return new HeavySlamAnimation(step, this);
			case "Crushing Swipe":
				return new CrushingSwipeAnimation(step, this);
			case "Block":
				return new BlockAnimation(step, this);
			case "Parry":
				return new ParryAnimation(step, this);
			case "Rattle Guard":
				return new RattleGuardAnimation(step, this);
			case "Whirling Strike":
				return new WhirlingStrikeAnimation(step, this);
			case "Soothing Song":
				return new SongAnimation(step, this, 0);
			case "Inspiring Song":
				return new SongAnimation(step, this, 1);
			case "Frightening Song":
				return new SongAnimation(step, this, 2);
			case "Discord":
				return new DiscordAnimation(step, this);
			case "Mockery":
				return new MockeryAnimation(step, this);
			case "Ice Ray":
				return new IceRayAnimation(step, this);
			case "Fireball":
				return new FireballAnimation(step, this);
			case "Magic Missile":
				return new MagicMissileAnimation(step, this);
			case "Piercing Arrow":
				return new PiercingArrowAnimation(step, this);
			case "Crippling Shot":
				return new CripplingShotAnimation(step, this);
			case "Rotting Aura":
				return new RottingAuraAnimation(step, this);
			case "Dark Bolt":
				return new DarkBoltAnimation(step, this);
			case "Leaf Dart":
				return new LeafDartAnimation(step, this);
			case "Devour Essence":
				return new DevourEssenceAnimation(step, this);
			case "Sonic Squeal":
				return new SonicSquealAnimation(step, this);
			case "Blood Peck":
				return new BloodPeckAnimation(step, this);
			case "Primal Roar":
				return new PrimalRoarAnimation(step, this);
			case "Soul Drain":
				return new SoulDrainAnimation(step, this);
			case "Mirror":
				return new MirrorAnimation(step, this);
			case "Ravenous Growth":
				return new RavenousGrowthAnimation(step, this);
			case "Entangle":
				return new EntangleAnimation(step, this);
			case "Power Cord":
			case "Power Chord":
				return new PowerChordAnimation(step, this);
			case "Summon":
				return new SummonAnimation(step, this);
			default:
				return this.updateAllHealth(step.GameState);
		}
	}

	createEnemyPuppet(type:string) {
		switch (type)
			{
				case "Skeleton":
					return new SkeletonPuppet();
					break;
				case "Goblin Archer":
					return new GoblinArcherPuppet();
					break;
				case "Forest Sprite":
					return new ForestSpritePuppet();
					break;
				case "Cave Bat":
					return new CaveBatPuppet();
					break;
				case "Gnombie Brute":
					return new GnombieBrutePuppet();
					break;
				case "Gnome Eater":
					return new GnomeEaterPuppet();
					break;
				case "Necrognomancer":
					return new NecrognomancerPuppet();
					break;
				default:
					return new SkeletonPuppet();
					console.log("Default triggered because it was", type);
			}
	}

	updateAllHealth(state:GameState) {
		let seq = new AnimationSequence();

		for (let player of state.PlayerCharacters)
		{
			let pvc = this.playerVisualComponents.get(player.Id);
			seq.steps.push(new FunctionStep(() => {pvc?.healthbar.changeHealth(player.Health);}));
		}
		for (let enemy of state.EnemyCharacters)
		{
			let evc = this.enemyVisualComponents.get(enemy.Id);
			seq.steps.push(new FunctionStep(() => {evc?.healthbar.changeHealth(enemy.Health);}));
		}

		seq.steps.push(new AnimationPause(1000));

		return seq;
	}
}

export interface CharacterVisualComponents {
	healthbar:HealthBar
	puppet:Puppet
}