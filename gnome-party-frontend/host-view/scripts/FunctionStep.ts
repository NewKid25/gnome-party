import AnimationStep from "./interfaces/AnimationStep";

export default
class FunctionStep implements AnimationStep {
	callback:Function
	play(): void {
		this.callback();
		if (this.onFinish)
			this.onFinish();
	}

	onFinish: Function | undefined;

	constructor(callback:Function) {
		this.callback = callback;
	}
}