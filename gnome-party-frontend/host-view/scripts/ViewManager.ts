import Konva from "konva";
import { Tween } from "konva/lib/Tween";
import AnimationSequence from "./AnimationSequence";
import SimultaneousAnimation from "./SimultaneousAnimation";
import TweenFromCurrent from "./TweenFromCurrent";
import LeapAnimation from "./animations/LeapAnimation";
import AnimationPause from "./AnimationPause";
import GnomePuppet from "./GnomePuppet";
import HealthBar from "./HealthBar";
import FunctionStep from "./FunctionStep";
import AnimationStep from "./interfaces/AnimationStep";
import { TurnStep } from "./interfaces/TurnStep";
import Puppet from "./interfaces/Puppet";
import SlashAnimation from "./animations/SlashAnimation";
import BoneSlashAnimation from "./animations/BoneSlashAnimation";
import { useSocketData } from "../../participant-view/stores/socketData";
import SkeletonPuppet from "./SkeletonPuppet";

export default
class ViewManager {

	socket:WebSocket

	stage:Konva.Stage

	mainLayer:Konva.Layer
	uiLayer:Konva.Layer
	
	playerVisualComponents:Map<string, CharacterVisualComponents> = new Map<string, CharacterVisualComponents>()
	enemyVisualComponents:Map<string, CharacterVisualComponents> = new Map<string, CharacterVisualComponents>()

	socketStore = useSocketData();

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
		/*
		var playerCharacters: Object[] = [
			{},
			{},
			{},
			{},
			{},
			{}
		]
		*/

		var playerCharacters = gameState.PlayerCharacters;

		for (let i = 0; i < playerCharacters.length; i++) {
			// Create GnomePuppet
			let puppet:GnomePuppet = new GnomePuppet();
			puppet.x(300);
			puppet.y((i + 1) * this.stage.height() / (playerCharacters.length + 1));

			this.mainLayer.add(puppet);
			// Create healthbar
			let healthbar:HealthBar = new HealthBar(playerCharacters[i].MaxHealth, {x: 30, y: puppet.height() / 2})
			healthbar.x(puppet.x() - puppet.width() /2 - 50);
			healthbar.y(puppet.y() - puppet.height() / 3.5);

			this.uiLayer.add(healthbar);

			this.playerVisualComponents.set(playerCharacters[i].Id, {puppet: puppet, healthbar: healthbar});
		}

		// Load enemy characters
		/*
		var enemyCharacters: Object[] = [
			{}
		]
		*/
		var enemyCharacters = gameState.EnemyCharacters;

		for (let i = 0; i < enemyCharacters.length; i++) {
			// Create puppet of corresponding enemy (using GnomePuppet as placeholder)
			let puppet:SkeletonPuppet = new SkeletonPuppet();
			puppet.x(this.stage.width() - 300);
			puppet.y((i + 1) * this.stage.height() / (enemyCharacters.length + 1));

			this.mainLayer.add(puppet);
			// Create healthbar
			let healthbar:HealthBar = new HealthBar(enemyCharacters[i].MaxHealth, {x: 30, y: puppet.height() / 2})
			healthbar.x(puppet.x() + puppet.width() /2 + 20);
			healthbar.y(puppet.y() - puppet.height() / 3.5);

			this.uiLayer.add(healthbar);

			this.enemyVisualComponents.set(enemyCharacters[i].Id, {puppet: puppet, healthbar: healthbar});
		}
	}

	handleMessage(msg:any) {
		if (msg.Subject == "host-game")
		{
			this.socketStore.gameSessionId = msg.Message.GameSessionId;
			this.socketStore.localPlayerId = msg.Message.Host.UserId;

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
				GameSessionId: this.socketStore.gameSessionId
			}))
		}

		if (msg.Subject == "begin-combat-encounter")
		{
			this.socketStore.encounterId = msg.Message.EncounterId;
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

		let sampleSteps:TurnStep[] = 
		[
			{
				"Request": {
				"EncounterId": "50b8c0cf-e032-4625-ba07-dad08231081b",
				"TargetCharacterId": "test-enemy",
				"SourceCharacterId": "0",
				"Action": "Slash"
				},
				"GameState": {
				"PlayerCharacters": [
					{
					"Id": "0",
					"Name": "Default Name",
					"Health": 1,
					"MaxHealth": 1,
					"ActionsDescriptions": [
						{
						"Name": "Slash",
						"Description": "default_action_description"
						},
						{
						"Name": "Block",
						"Description": "default_action_description"
						}
					]
					},
					{
					"Id": "1",
					"Name": "Default Name",
					"Health": 1,
					"MaxHealth": 1,
					"ActionsDescriptions": [
						{
						"Name": "Slash",
						"Description": "default_action_description"
						},
						{
						"Name": "Block",
						"Description": "default_action_description"
						}
					]
					}
				],
				"EnemyCharacters": [
					{
					"Id": "test-enemy",
					"Name": "skeleton",
					"Health": 12,
					"MaxHealth": 10,
					"ActionsDescriptions": [
						{
						"Name": "punch",
						"Description": "A weak punch"
						}
					]
					}
				]
				},
				"Events": []

			},
			{
				"Request": {
				"EncounterId": "50b8c0cf-e032-4625-ba07-dad08231081b",
				"TargetCharacterId": "test-enemy",
				"SourceCharacterId": "1",
				"Action": "Slash"
				},
				"GameState": {
				"PlayerCharacters": [
					{
					"Id": "0",
					"Name": "Default Name",
					"Health": 1,
					"MaxHealth": 1,
					"ActionsDescriptions": [
						{
						"Name": "Slash",
						"Description": "default_action_description"
						},
						{
						"Name": "Block",
						"Description": "default_action_description"
						}
					]
					},
					{
					"Id": "1",
					"Name": "Default Name",
					"Health": 1,
					"MaxHealth": 1,
					"ActionsDescriptions": [
						{
						"Name": "Slash",
						"Description": "default_action_description"
						},
						{
						"Name": "Block",
						"Description": "default_action_description"
						}
					]
					}
				],
				"EnemyCharacters": [
					{
					"Id": "test-enemy",
					"Name": "skeleton",
					"Health": 8,
					"MaxHealth": 10,
					"ActionsDescriptions": [
						{
						"Name": "punch",
						"Description": "A weak punch"
						}
					]
					}
				]
				},
				"Events": []
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
		}
	}
}

export interface CharacterVisualComponents {
	healthbar:HealthBar
	puppet:Puppet
}