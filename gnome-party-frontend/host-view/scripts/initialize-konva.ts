export default initializeKonva

import Konva from "konva";
import { Tween } from "konva/lib/Tween";
import AnimationSequence from "./AnimationSequence";
import SimultaneousAnimation from "./SimultaneousAnimation";
import TweenFromCurrent from "./TweenFromCurrent";
import LeapAnimation from "./animations/LeapAnimation";
import AnimationPause from "./AnimationPause";
import GnomePuppet from "./GnomePuppet";

function initializeKonva() {
	var container:HTMLDivElement = document.getElementById("konva-container") as HTMLDivElement;

	// first we need to create a stage
	var stage = new Konva.Stage({
		container: 'konva-container', // id of container <div>
		width: container.clientWidth,
		height: container.clientHeight
	});

	// then create layer
	var layer = new Konva.Layer();

	// create our shape
	var circle = new Konva.Circle({
		x: stage.width() / 2,
		y: stage.height() / 2,
		radius: 70,
		fill: 'red',
		stroke: 'black',
		strokeWidth: 4,
	});

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

	
	// add the shape to the layer
	layer.add(circle);
	layer.add(gnome);
	layer.add(otherCircle);

	// add the layer to the stage
	stage.add(layer);


	var tweenColor:TweenFromCurrent = new TweenFromCurrent({
		node: gnome,
		duration: 1,
		fill: 'green'
	})	

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

	let circleLeapAnimation:LeapAnimation = new LeapAnimation({
		leapingNode: circle,
		// destination: {x: circle.position().x + 300, y: circle.position().y - 200},
		destination: otherCircle,
		landingAnimation: new AnimationPause(100),
		leapDuration: 0.1
	})


	let sequence:AnimationSequence = new AnimationSequence([
		new AnimationPause(1000),
		flipAnimation,
		circleLeapAnimation
	])
	
	sequence.play();
	console.log(circle.offset());


}