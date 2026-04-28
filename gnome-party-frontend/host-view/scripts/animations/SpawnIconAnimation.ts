import Konva from "konva";
import AnimationSequence from "../AnimationSequence";
import AnimationStep from "../interfaces/AnimationStep";
import SimultaneousAnimation from "../SimultaneousAnimation";
import { Vector2d } from "konva/lib/types";
import AnimationPause from "../AnimationPause";
import FunctionStep from "../FunctionStep";
import TweenFromCurrent from "../TweenFromCurrent";

type SpawnIconAnimationParams = {
	node:Konva.Shape,
	position:Vector2d | {x: Function|Number, y: Function|Number},
	layer:Konva.Layer,
}

export default
class SpawnIconAnimation implements AnimationStep {
	
	private node: Konva.Shape;

	private sequence:AnimationSequence;

	play():void {
		this.sequence.onFinish = this.onFinish;
		this.sequence.play();
	}

	onFinish: Function | undefined;

	constructor({node, position, layer,}:SpawnIconAnimationParams) {
		this.node = node;

		layer.add(this.node);

		this.sequence = new AnimationSequence([
			new TweenFromCurrent({
				node: this.node,
				opacity: 1.0,
				y: position.y,
				duration: 0.2
			}),
			new AnimationPause(1000),
			new TweenFromCurrent({
				node: this.node,
				opacity: 0.0,
				duration: 0.2
			})
		])
	}
}