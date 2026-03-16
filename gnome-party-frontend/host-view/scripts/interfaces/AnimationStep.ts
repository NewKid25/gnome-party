export default

interface AnimationStep {
	play():void
	onFinish:Function | undefined
}