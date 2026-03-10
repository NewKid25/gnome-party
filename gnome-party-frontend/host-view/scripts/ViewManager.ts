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

export default
class ViewManager {

	stage:Konva.Stage

	mainLayer:Konva.Layer
	uiLayer:Konva.Layer
	
	playerVisualComponents:Map<number, CharacterVisualComponents> = new Map<number, CharacterVisualComponents>()
	enemyVisualComponents:Map<number, CharacterVisualComponents> = new Map<number, CharacterVisualComponents>()


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
	
	
		// test shapes
		/*
		var otherCircle = new Konva.Circle({
			x: this.stage.width() / 2 + 600,
			y: this.stage.height() / 2 - 200,
			radius: 50,
			fill: 'orange',
			stroke: 'black',
			strokeWidth: 4,
		})
	
		let gnome:GnomePuppet = new GnomePuppet();
		gnome.x(this.stage.width() / 2);
		gnome.y(this.stage.height() / 2);
	
		let gnomeHealth:HealthBar = new HealthBar(20, {x: 30, y:150});
		gnomeHealth.x(gnome.x() - gnome.width() / 1.75)
		gnomeHealth.y(gnome.y() - 85)
	
		let circleHealth:HealthBar = new HealthBar(20, {x: 30, y:150});
		circleHealth.x(otherCircle.x() + otherCircle.width() / 1.75)
		circleHealth.y(otherCircle.y() - 85)
	
		
		// add the shape to the layer
		this.mainLayer.add(gnome);
		this.mainLayer.add(otherCircle);
		this.uiLayer.add(gnomeHealth);
		this.uiLayer.add(circleHealth);
	
	
	
		let leapAnimation:LeapAnimation = new LeapAnimation({
			leapingNode: gnome,
			// destination: {x: circle.position().x + 300, y: circle.position().y - 200},
			destination: otherCircle,
			landingAnimation: new SimultaneousAnimation([new AnimationPause(100), new FunctionStep(() => {circleHealth.changeHealth(8)})]),
			leapDuration: 1.5
		})
	
		let flipAnimation:SimultaneousAnimation = new SimultaneousAnimation([
			leapAnimation,
			new Konva.Tween({
				node: gnome,
				rotation: 360,
				duration: .75
			})
		])
	
	
		let sequence:AnimationSequence = new AnimationSequence([
			new AnimationPause(1000),
			flipAnimation,
			new FunctionStep(() => {gnomeHealth.changeHealth(15)})
		])
		
		sequence.play();
		*/
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

			this.playerVisualComponents.set(i, {puppet: puppet, healthbar: healthbar});
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

			this.enemyVisualComponents.set(i, {puppet: puppet, healthbar: healthbar});
		}
	}

	gameLoop()
	{
	}

	testAnimation()
	{
		let anims:AnimationStep[] = []
		for (const pair of this.playerVisualComponents) {
			const pvc:CharacterVisualComponents = pair[1];

			const enemy:CharacterVisualComponents = (this.enemyVisualComponents.get(0) as CharacterVisualComponents)

			let anim:LeapAnimation = new LeapAnimation({
				leapingNode: pvc.puppet, 
				destination: enemy.puppet, 
				landingAnimation: new FunctionStep(() => { enemy.healthbar.changeHealth(20 - (pair[0] + 1) * 2); })
			});

			anims.push(anim);
		}

		let sequence:AnimationSequence = new AnimationSequence(anims);

		sequence.play();
	}
}

interface CharacterVisualComponents {
	healthbar:HealthBar
	puppet:Konva.Group
}
