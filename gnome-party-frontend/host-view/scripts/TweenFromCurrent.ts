import AnimationStep from "./interfaces/AnimationStep";
import Konva from "konva";

export default
class TweenFromCurrent implements AnimationStep {
	tweenConfigGenerator:Object
	play(): void {
		let node = Object.entries(this.tweenConfigGenerator).filter(([key, val]) => key == "node")[0][1];
		var tweenConfig:Konva.TweenConfig;
		if (node instanceof Function)
		{
			tweenConfig = {node: node()}
		}
		else
		{
			tweenConfig = {node: node}
		}

		Object.entries(this.tweenConfigGenerator).forEach(([key, value]) => {
			if (value instanceof Function)
			{
				tweenConfig[key] = value();
			}
			else
			{
				tweenConfig[key] = value;
			}
		});

		var tween:Konva.Tween = new Konva.Tween(tweenConfig);
		tween.onFinish = () => {
			tween.destroy(); 
			if (this.onFinish)
			{
				this.onFinish();
			}
		};
		tween.play();
	}
	onFinish: Function | undefined;

	constructor(tweenConfigGenerator:Object) {
		this.tweenConfigGenerator = tweenConfigGenerator;
	}
}