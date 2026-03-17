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
import { useEncounterData } from "../../participant-view/stores/encounterData";
import SkeletonPuppet from "./SkeletonPuppet";

export default
class ViewManager {

	socket:WebSocket

	stage:Konva.Stage

	mainLayer:Konva.Layer
	uiLayer:Konva.Layer
	
	playerVisualComponents:Map<string, CharacterVisualComponents> = new Map<string, CharacterVisualComponents>()
	enemyVisualComponents:Map<string, CharacterVisualComponents> = new Map<string, CharacterVisualComponents>()

	encounterData = useEncounterData();

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
		if (msg.GameSessionId) {
			this.encounterData.gameSessionId = msg.GameSessionId;
		}
		if (msg.UserId) {
			this.encounterData.localPlayerId = msg.localPlayerId;
		}
		if (msg.EncounterId) {
			this.encounterData.encounterId = msg.EncounterId;

			this.loadEncounter(msg.GameState);
		}
		if (msg[0] && msg[0].Request && msg.length == this.playerVisualComponents.size)
		{
			console.log("yuh");
			if (msg[0].Request) {
				console.log("YUHHHH");
				this.processTurn(msg);
			}
		}
	}

	processTurn(turn:TurnStep[])
	{
		let animations:AnimationStep[] = [];
		let finalStep:TurnStep|undefined;
		for (let step of turn)
		{
			let animation:AnimationStep | undefined = this.instantiateActionAnimation(step);
			if (animation) animations.push(animation);
			finalStep = step;
		}
		
		if (finalStep)
		{
			for (let event of finalStep.Events)
			{
				console.log(event);
				if (event.event == "damage" && this.enemyVisualComponents.has(event.params.SourceId))
				{
					let sourceNode:Puppet | undefined = this.enemyVisualComponents.get(event.params.SourceId)?.puppet;
					let target:CharacterVisualComponents | undefined = this.playerVisualComponents.get(event.params.TargetId);
					if (sourceNode && target)
					{
						let animation:AnimationStep = new LeapAnimation({
							leapingNode: sourceNode,
							destination: target.puppet,
							leapDuration: 1,
							jumpHeight: 10,
							landingAnimation: new FunctionStep(() => {
								target.healthbar.changeHealth(target.healthbar.getHealth() - event.params.DamageAmount)
							})
						})

						animations.push(animation);
					}
					else
					{
						console.log("SOMETHING'S MISSING")
					}

				}
				else 
				{
					console.log("COULDN'T FIND ENEMY IN THE KEYS")
				}
			}
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
				}
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
				}
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
		}
	}
}

export interface CharacterVisualComponents {
	healthbar:HealthBar
	puppet:Puppet
}