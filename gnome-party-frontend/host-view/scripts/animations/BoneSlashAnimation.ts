import { Group } from "konva/lib/Group";
import FunctionStep from "../FunctionStep";
import GnomePuppet from "../GnomePuppet";
import HealthBar from "../HealthBar";
import AnimationStep from "../interfaces/AnimationStep";
import Puppet from "../interfaces/Puppet";
import { GameState, TurnStep } from "../interfaces/TurnStep";
import ViewManager from "../ViewManager";
import LeapAnimation from "./LeapAnimation";

export default
class SlashAnimation implements AnimationStep
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
		let enemyPuppet:Group | undefined = vm.enemyVisualComponents.get(step.Request.SourceCharacterId)?.puppet;
		let playerPuppet: Group | undefined = vm.playerVisualComponents.get(step.Request.TargetCharacterId)?.puppet;
		let playerHealth: HealthBar | undefined = vm.playerVisualComponents.get(step.Request.TargetCharacterId)?.healthbar;
		
		if (enemyPuppet && playerPuppet && playerHealth)
		{
			this.leapAnim = new LeapAnimation({
				leapingNode: enemyPuppet,
				destination: playerPuppet,
				leapDuration: 1,
				landingAnimation: new FunctionStep(() => {
					playerHealth.changeHealth(step.GameState.PlayerCharacters.find((v, i, o) => v.Id == step.Request.TargetCharacterId)?.Health ?? 0)
				})
			})
		} else throw TypeError
	}
}