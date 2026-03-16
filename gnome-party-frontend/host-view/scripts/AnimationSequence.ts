import AnimationStep from "./interfaces/AnimationStep";

export default

class AnimationSequence implements AnimationStep {
	steps:Array<AnimationStep> = [];

	play():void {
		if (this.steps.length > 0)
		{
			this.steps.forEach((step:AnimationStep, i:number) => {
				if (i < this.steps.length - 1) {
					step.onFinish = () => {this.steps[i + 1].play();}
				} else {
					step.onFinish = this.onFinish;
				}
			});

			this.steps[0].play();
		}
		
	}
	onFinish: Function | undefined = undefined;

	constructor(steps:Array<AnimationStep>=[]){
		this.steps = steps;
	}
}