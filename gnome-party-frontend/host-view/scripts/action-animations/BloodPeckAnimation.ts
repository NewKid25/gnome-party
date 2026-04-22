import { Group } from "konva/lib/Group";
import FunctionStep from "../FunctionStep";
import GnomePuppet from "../puppets/GnomePuppet";
import HealthBar from "../HealthBar";
import AnimationStep from "../interfaces/AnimationStep";
import Puppet from "../interfaces/Puppet";
import { GameState, TurnStep } from "../interfaces/TurnStep";
import ViewManager from "../ViewManager";
import LeapAnimation from "../animations/LeapAnimation";
import SimultaneousAnimation from "../SimultaneousAnimation";
import AnimationSequence from "../AnimationSequence";
import TweenFromCurrent from "../TweenFromCurrent";

export default
class BloodPeckAnimation implements AnimationStep
{
	leapAnim:LeapAnimation
	play() 
	{
		this.leapAnim.onFinish = this.onFinish
		this.leapAnim.play();
	}
	onFinish: Function | undefined;

	constructor(step:TurnStep, vm:ViewManager) 
	{
		console.log(step.Request.SourceCharacterId, step.Request.TargetCharacterId)
		let enemyPuppet:Puppet | undefined = vm.enemyVisualComponents.get(step.Request.SourceCharacterId)?.puppet;
		let enemyHealth: HealthBar | undefined = vm.enemyVisualComponents.get(step.Request.SourceCharacterId)?.healthbar;
		let playerPuppet: Puppet | undefined = vm.playerVisualComponents.get(step.Request.TargetCharacterId)?.puppet;
		let playerHealth: HealthBar | undefined = vm.playerVisualComponents.get(step.Request.TargetCharacterId)?.healthbar;
		
		if (enemyPuppet && playerPuppet && playerHealth && enemyHealth)
		{
			this.leapAnim = new LeapAnimation({
				leapingNode: enemyPuppet,
				destination: playerPuppet,
				leapDuration: 0.7,
				jumpHeight: 10,
				landingAnimation: new SimultaneousAnimation([
					new FunctionStep(() => {
						playerHealth.changeHealth(step.GameState.PlayerCharacters.find((v, i, o) => v.Id == step.Request.TargetCharacterId)?.Health ?? 0)
					}),
					new FunctionStep(() => {
						enemyHealth.changeHealth(step.GameState.EnemyCharacters.find((v, i, o) => v.Id == step.Request.SourceCharacterId)?.Health ?? 0)
					}),
				]) 
			})
		} else throw TypeError
	}
}