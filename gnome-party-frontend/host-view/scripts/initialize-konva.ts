export default initializeKonva

import Konva from "konva";
import { Tween } from "konva/lib/Tween";
import AnimationSequence from "./AnimationSequence";
import SimultaneousAnimation from "./SimultaneousAnimation";
import TweenFromCurrent from "./TweenFromCurrent";
import LeapAnimation from "./animations/LeapAnimation";

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
		x: stage.width() / 2 - 400,
		y: stage.height() / 2 - 200,
		radius: 50,
		fill: 'orange',
		stroke: 'black',
		strokeWidth: 4,
	})

	// add the shape to the layer
	layer.add(circle);
	layer.add(otherCircle);

	// add the layer to the stage
	stage.add(layer);


	var tweenColor:TweenFromCurrent = new TweenFromCurrent({
		node: circle,
		duration: 1,
		fill: 'green'
	})	

	let leapAnimation:LeapAnimation = new LeapAnimation({
		leapingNode: circle,
		// destination: {x: circle.position().x + 300, y: circle.position().y - 200},
		destination: otherCircle,
		landingAnimation: tweenColor
	})

	leapAnimation.play();
}