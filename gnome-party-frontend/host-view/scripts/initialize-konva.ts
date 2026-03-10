export default initializeKonva

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

function initializeKonva() {
	const container:HTMLDivElement = document.getElementById("konva-container") as HTMLDivElement;

	// first we need to create a stage
	var stage = new Konva.Stage({
		container: 'konva-container', // id of container <div>
		width: container.clientWidth,
		height: container.clientHeight
	});

	// then create layers
	var mainLayer = new Konva.Layer();


	var otherCircle = new Konva.Circle({
		x: stage.width() / 2 + 600,
		y: stage.height() / 2 - 200,
		radius: 50,
		fill: 'orange',
		stroke: 'black',
		strokeWidth: 4,
	})

	let gnome:GnomePuppet = new GnomePuppet();
	gnome.x(stage.width() / 2);
	gnome.y(stage.height() / 2);

	let health:HealthBar = new HealthBar(20, {x: 30, y:150});
	health.x(gnome.x() - gnome.width() / 1.75)
	health.y(gnome.y() - 85)

	
	// add the shape to the layer
	mainLayer.add(gnome);
	mainLayer.add(otherCircle);
	mainLayer.add(health);

	// add the layer to the stage
	stage.add(mainLayer);

	let leapAnimation:LeapAnimation = new LeapAnimation({
		leapingNode: gnome,
		// destination: {x: circle.position().x + 300, y: circle.position().y - 200},
		destination: otherCircle,
		landingAnimation: new AnimationPause(100),
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
		new FunctionStep(() => {health.changeHealth(10)})
	])
	
	sequence.play();

}