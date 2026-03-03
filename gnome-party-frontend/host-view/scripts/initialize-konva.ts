export default initializeKonva

import Konva from "konva";
import { Tween } from "konva/lib/Tween";
import AnimationSequence from "./AnimationSequence";
import SimultaneousAnimation from "./SimultaneousAnimation";
import TweenFromCurrent from "./TweenFromCurrent";

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

	// add the shape to the layer
	layer.add(circle);

	// add the layer to the stage
	stage.add(layer);

	var tween:TweenFromCurrent = new TweenFromCurrent({
		node: circle,
		duration: 1,
		onFinish: () => {console.log("Done!")},
		x: circle.position().x + 20
	});

	var tween2:TweenFromCurrent = new TweenFromCurrent({
		node: circle,
		duration: 1,
		x: circle.position().x - 200
	})

	var tweenColor:TweenFromCurrent = new TweenFromCurrent({
		node: circle,
		duration: 4,
		fill: 'green'
	})

	var sequence:AnimationSequence = new AnimationSequence([
		tween,
		tween2,
	]);

	var simul:SimultaneousAnimation = new SimultaneousAnimation([
		sequence,
		tweenColor
	]);

	var sequence2:AnimationSequence = new AnimationSequence([
		simul,
		new TweenFromCurrent({
			node: circle,
			duration: 1,
			onFinish: () => {console.log("Done!")},
			x: (() => circle.position().x + 20)
		}),
	]);


	sequence2.play();
}