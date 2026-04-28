import Konva from "konva";
import AnimationSequence from "../AnimationSequence";
import AnimationStep from "../interfaces/AnimationStep";
import SimultaneousAnimation from "../SimultaneousAnimation";
import { Vector2d } from "konva/lib/types";
import AnimationPause from "../AnimationPause";
import FunctionStep from "../FunctionStep";
import TweenFromCurrent from "../TweenFromCurrent";

type TextPopupAnimationParams = {
	text:string,
	position:Vector2d,
	layer:Konva.Layer,
	textFill?:string,
	textOutline?:string,
}

export default
class TextPopupAnimation implements AnimationStep {
	
	private textNode: Konva.Text;

	// private text:String = "";
	// private textFill:String = "#ffffff";
	// private textOutline:String = "#0000000";

	private sequence:AnimationSequence;

	play():void {
		this.sequence.onFinish = this.onFinish;
		this.sequence.play();
	}

	onFinish: Function | undefined;

	constructor({text, position, layer, textFill = "#ffffff", textOutline = "#000000"}:TextPopupAnimationParams) {

		this.textNode = new Konva.Text({
			text: text,
			x: position.x,
			y: position.y + 20,
			fill: textFill,
			stroke: textOutline,
			strokeWidth: 1,
			fontStyle: "800",
			opacity: 0.0,
			fontSize: 20,
		})

		layer.add(this.textNode);

		this.sequence = new AnimationSequence([
			new TweenFromCurrent({
				node: this.textNode,
				opacity: 1.0,
				y: position.y,
				duration: 0.2
			}),
			new AnimationPause(1000),
			new TweenFromCurrent({
				node: this.textNode,
				opacity: 0.0,
				duration: 0.2
			})
		])
	}
}