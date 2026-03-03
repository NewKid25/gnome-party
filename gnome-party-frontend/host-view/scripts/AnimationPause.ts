import AnimationStep from "./interfaces/AnimationStep";

export default
class AnimationPause implements AnimationStep {
	duration:number = 0;
	play(): void {
		setTimeout(() => { if (this.onFinish) this.onFinish()}, this.duration);
	}

	onFinish: Function | undefined;

	constructor(duration:number=0) {
		this.duration = duration;
	}
}