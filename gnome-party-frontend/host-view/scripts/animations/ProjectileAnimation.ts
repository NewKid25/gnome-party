import Konva from "konva";
import AnimationSequence from "../AnimationSequence";
import AnimationStep from "../interfaces/AnimationStep";
import SimultaneousAnimation from "../SimultaneousAnimation";
import { Vector2d } from "konva/lib/types";
import AnimationPause from "../AnimationPause";
import FunctionStep from "../FunctionStep";
import TweenFromCurrent from "../TweenFromCurrent";

type ProjectileAnimationParams = {
	node:Konva.Shape,
	startPos:Vector2d | {x: Function|Number, y: Function|Number},
	endPos:Vector2d | {x: Function|Number, y: Function|Number},
	layer:Konva.Layer,
	duration?:number,
	impactAnimation?:AnimationStep
}

export default
class ProjectileAnimation implements AnimationStep {
	private node: Konva.Shape;

	private sequence:AnimationSequence;

	play():void {
		this.sequence.onFinish = this.onFinish;
		this.sequence.play();
	}

	onFinish: Function | undefined;

	constructor({node, startPos, endPos, layer, duration = 1.2, impactAnimation = new AnimationPause(0)}:ProjectileAnimationParams) {
		this.node = node;

		

		this.sequence = new SimultaneousAnimation([
			new AnimationSequence([
				new FunctionStep(() => {
					layer.add(this.node);
					this.node.position({
						x: (startPos.x instanceof Function) ? startPos.x() : startPos.x,
						y: (startPos.y instanceof Function) ? startPos.y() : startPos.y,
					})
				}),

				new TweenFromCurrent({
					node: this.node,
					opacity: 1.0,
					duration: 0.2
				}),
				new AnimationPause((duration - 0.4) * 1000),
				new TweenFromCurrent({
					node: this.node,
					opacity: 0.0,
					duration: 0.2
				})
			]),
			new AnimationSequence([
				new TweenFromCurrent({
					node: this.node,
					x: endPos.x,
					y: endPos.y,
					duration: duration
				}),
				impactAnimation
			])
		]) 
	}
}