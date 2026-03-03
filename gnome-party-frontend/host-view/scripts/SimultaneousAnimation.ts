import AnimationStep from "./interfaces/AnimationStep";

export default

class SimultaneousAnimation implements AnimationStep {
	steps:Array<AnimationStep> = [];

	private _completedSteps:number = 0;

	get completedSteps():number { return this._completedSteps; }
	set completedSteps(newVal:number) 
	{
		console.log("Steps completed: ", newVal);
		if (newVal == this.steps.length)
		{
			if (this.onFinish)
			{
				this.onFinish();
			}
			this._completedSteps = 0;
		} 
		else 
		{
			this._completedSteps = newVal;
		}
	}

	play():void 
	{
		if (this.steps.length > 0)
		{
			this.steps.forEach((step:AnimationStep, i:number) => {
				step.onFinish = () => {this.completedSteps++;}
				step.play();
			});
		}
	}
	onFinish: Function | undefined = undefined;

	constructor(steps:Array<AnimationStep>=[]){
		this.steps = steps;
	}
}