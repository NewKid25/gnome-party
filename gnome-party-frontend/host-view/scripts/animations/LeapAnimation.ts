import Konva from "konva";
import AnimationSequence from "../animationSequence";
import AnimationStep from "../interfaces/AnimationStep";
import SimultaneousAnimation from "../SimultaneousAnimation";
import { Easings } from "konva/lib/Tween";
import TweenFromCurrent from "../TweenFromCurrent";
import AnimationPause from "../AnimationPause";

type LeapAnimationParams = {
	leapingNode:Konva.Node, 
	destination:Konva.Node | Konva.Vector2d, 
	jumpHeight?:number, 
	leapDuration?:number, 
	landingAnimation?:AnimationStep
}

export default
class LeapAnimation implements AnimationStep {
	node:Konva.Node;

	private animationSequence:AnimationSequence;

	play():void{
		this.animationSequence.onFinish = this.onFinish
		this.animationSequence.play();
	}

	onFinish: Function | undefined;

	constructor({leapingNode, destination, jumpHeight=30, leapDuration=1, landingAnimation=new AnimationPause()} : LeapAnimationParams)
	{
		this.node = leapingNode;


		let destinationPos:Konva.Vector2d;
		if (destination instanceof Konva.Node)
		{
			destinationPos = destination.position();
			if (leapingNode.position().x < destinationPos.x)
			{
				destinationPos.x -= (destination.width() + leapingNode.width()) / 2 * 1.25;
			}
			else
			{
				destinationPos.x += (destination.width() + leapingNode.width()) / 2 * 1.25;
			}
		}
		else
		{
			destinationPos = destination;
		}

		let higher:number;

		if (destinationPos.y < leapingNode.position().y)
		{
			higher = destinationPos.y;
		}
		else
		{
			higher = leapingNode.position().y;
		}

		this.animationSequence = new AnimationSequence([
			// Leap to target
			new SimultaneousAnimation([
				// Up and down
				new AnimationSequence([
					new TweenFromCurrent({
						node: leapingNode,
						y: higher - jumpHeight,
						duration: leapDuration / 4,
						easing: () => Easings.EaseOut
					}),
					new TweenFromCurrent({
						node: leapingNode,
						y: destinationPos.y,
						duration: leapDuration / 4,
						easing: () => Easings.EaseIn
					}),
				]),
				// Across
				new TweenFromCurrent({
					node: leapingNode,
					x: destinationPos.x,
					duration: leapDuration / 2
				})
			]),
			// Landing animation if real
			landingAnimation,
			// Leap back to original position
			new SimultaneousAnimation([
				// Up and down
				new AnimationSequence([
					new TweenFromCurrent({
						node: leapingNode,
						y: higher - jumpHeight,
						duration: leapDuration / 4,
						easing: () => Easings.EaseOut
					}),
					new TweenFromCurrent({
						node: leapingNode,
						y: leapingNode.position().y,
						duration: leapDuration / 4,
						easing: () => Easings.EaseIn
					}),
				]),
				// Across
				new TweenFromCurrent({
					node: leapingNode,
					x: leapingNode.position().x,
					duration: leapDuration / 2
				})
			]),
		])
	}
}