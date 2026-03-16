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

export default
class ViewManager {

	stage:Konva.Stage

	mainLayer:Konva.Layer
	uiLayer:Konva.Layer
	
	playerVisualComponents:Map<string, CharacterVisualComponents> = new Map<string, CharacterVisualComponents>()
	enemyVisualComponents:Map<string, CharacterVisualComponents> = new Map<string, CharacterVisualComponents>()


	constructor() {
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
	}
	
	loadEncounter()
	{
		// Load player characters
		var playerCharacters: Object[] = [
			{},
			{},
			{},
			{},
			{},
			{}
		]

		for (let i = 0; i < playerCharacters.length; i++) {
			// Create GnomePuppet
			let puppet:GnomePuppet = new GnomePuppet();
			puppet.x(300);
			puppet.y((i + 1) * this.stage.height() / (playerCharacters.length + 1));

			this.mainLayer.add(puppet);
			// Create healthbar
			let healthbar:HealthBar = new HealthBar(20, {x: 30, y: puppet.height() / 2})
			healthbar.x(puppet.x() - puppet.width() /2 - 50);
			healthbar.y(puppet.y() - puppet.height() / 3.5);

			this.uiLayer.add(healthbar);

			this.playerVisualComponents.set(i.toString(), {puppet: puppet, healthbar: healthbar});
		}

		// Load enemy characters
		var enemyCharacters: Object[] = [
			{}
		]

		for (let i = 0; i < enemyCharacters.length; i++) {
			// Create puppet of corresponding enemy (using GnomePuppet as placeholder)
			let puppet:GnomePuppet = new GnomePuppet();
			puppet.x(this.stage.width() - 300);
			puppet.y((i + 1) * this.stage.height() / (enemyCharacters.length + 1));

			this.mainLayer.add(puppet);
			// Create healthbar
			let healthbar:HealthBar = new HealthBar(20, {x: 30, y: puppet.height() / 2})
			healthbar.x(puppet.x() + puppet.width() /2 + 20);
			healthbar.y(puppet.y() - puppet.height() / 3.5);

			this.uiLayer.add(healthbar);

			this.enemyVisualComponents.set("test-enemy", {puppet: puppet, healthbar: healthbar});
		}
	}

	gameLoop()
	{
	}

	processTurn(turn:TurnStep[])
	{
		let animations:AnimationStep[] = [];
		for (let step of turn)
		{
			let animation:AnimationStep | undefined = this.instantiateActionAnimation(step);
			if (animation) animations.push(animation);
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