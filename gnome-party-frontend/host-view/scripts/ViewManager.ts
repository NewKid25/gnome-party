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
import SlashAnimation from "./animations/SlashAnimation";
import BoneSlashAnimation from "./animations/BoneSlashAnimation";
import { useEncounterData } from "../../participant-view/stores/encounterData";
import SkeletonPuppet from "./puppets/SkeletonPuppet";
import GoblinArcherPuppet from "./puppets/GoblinArcherPuppet";
import { C } from "vue-router/dist/options-CjwwR_07.cjs";
import ForestSpritePuppet from "./puppets/ForestSpritePuppet";
import CaveBatPuppet from "./puppets/CaveBatPuppet";
import GnombieBrutePuppet from "./puppets/GnombieBrutePuppet";
import GnomeEaterPuppet from "./puppets/GnomeEaterPuppet";
import NecrognomancerPuppet from "./puppets/NecrognomancerPuppet";

export default
class ViewManager {

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
		const HEALTHBAR_HEIGHT = 120;

		// Load player characters
		var playerCharacters = gameState.PlayerCharacters;

		for (let i = 0; i < playerCharacters.length; i++) {
			// Create GnomePuppet
			let puppet:GnomePuppet = new GnomePuppet();
			puppet.x(300);
			puppet.y((i + 1) * this.stage.height() / (playerCharacters.length + 1));

			this.mainLayer.add(puppet);
			// Create healthbar
			let healthbar:HealthBar = new HealthBar(playerCharacters[i].MaxHealth, {x: 30, y: HEALTHBAR_HEIGHT})
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
			switch (type)
			{
				case "Skeleton":
					puppet = new SkeletonPuppet();
					break;
				case "Goblin Archer":
					puppet = new GoblinArcherPuppet();
					break;
				case "Forest Sprite":
					puppet = new ForestSpritePuppet();
					break;
				case "Cave Bat":
					puppet = new CaveBatPuppet();
					break;
				case "Gnombie Brute":
					puppet = new GnombieBrutePuppet();
					break;
				case "Gnome Eater":
					puppet = new GnomeEaterPuppet();
					break;
				case "Necrognomancer":
					puppet = new NecrognomancerPuppet();
					break;
				default:
					puppet = new SkeletonPuppet();
					console.log("Default triggered because it was", enemyCharacters[i].CharacterType);
			}
			puppet.x(this.stage.width() - 300);
			puppet.y((i + 1) * this.stage.height() / (enemyCharacters.length + 1));

			this.mainLayer.add(puppet);
			// Create healthbar
			let healthbar:HealthBar = new HealthBar(enemyCharacters[i].MaxHealth, {x: 30, y: HEALTHBAR_HEIGHT})
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

		
		let gameState:GameState = {"EnemyCharacters":[{"CharacterType":"Necrognomancer","Health":30,"Id":"e1","MaxHealth":30,"Name":"Goblin Archer","ActionsDescriptions":[{"Description":"Deal 6 damage to target enemy","Name":"Bone Slash"},{"Description":"Reduce damage by 50% for one turn","Name":"Rattle Guard"}],"StatusEffects":[]}],"PlayerCharacters":[{"CharacterType":"Mage","Health":20,"Id":"p1","MaxHealth":20,"Name":"Mage","ActionsDescriptions":[{"Description":"Deal damage to the target and then burn the target and adjacent allies for 3 turns","Name":"Fireball"}],"StatusEffects":[]}]};
		
		this.loadEncounter(gameState);

		let sampleSteps:TurnStep[] = 
		[
			{
				Request:   {
					EncounterId:       "",
					TargetCharacterId: "p1",
					SourceCharacterId: "e1",
					Action:            "Glump",
				},
				GameState: {
					PlayerCharacters: [
						{
							CharacterType:      "Mage",
							Id:                  "p1",
							Name:                "Mage",
							Health:              10,
							MaxHealth:           20,
							ActionsDescriptions: [],
							StatusEffects:      [],
						}
					],
					EnemyCharacters:  [
						{
							CharacterType:      "Goblin Archer",
							Id:                  "e1",
							Name:                "Goblin Archer",
							Health:              30,
							MaxHealth:           30,
							ActionsDescriptions: [],
							StatusEffects:      [],
						}
					],
				},
				Events: [],
			},
			{
				Request:   {
					EncounterId:       "",
					TargetCharacterId: "e1",
					SourceCharacterId: "p1",
					Action:            "Glump",
				},
				GameState: {
					PlayerCharacters: [
						{
							CharacterType:      "Mage",
							Id:                  "p1",
							Name:                "Mage",
							Health:              10,
							MaxHealth:           20,
							ActionsDescriptions: [],
							StatusEffects:      [],
						}
					],
					EnemyCharacters:  [
						{
							CharacterType:      "Goblin Archer",
							Id:                  "e1",
							Name:                "Goblin Archer",
							Health:              15,
							MaxHealth:           30,
							ActionsDescriptions: [],
							StatusEffects:      [],
						}
					],
				},
				Events: [],
			}
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
			default:
				let seq = new AnimationSequence();

				for (let player of step.GameState.PlayerCharacters)
				{
					let pvc = this.playerVisualComponents.get(player.Id);
					seq.steps.push(new FunctionStep(() => {pvc?.healthbar.changeHealth(player.Health);}));
				}
				for (let enemy of step.GameState.EnemyCharacters)
				{
					let evc = this.enemyVisualComponents.get(enemy.Id);
					seq.steps.push(new FunctionStep(() => {evc?.healthbar.changeHealth(enemy.Health);}));
				}

				seq.steps.push(new AnimationPause(1000));

				return seq;
		}
	}
}

export interface CharacterVisualComponents {
	healthbar:HealthBar
	puppet:Puppet
}