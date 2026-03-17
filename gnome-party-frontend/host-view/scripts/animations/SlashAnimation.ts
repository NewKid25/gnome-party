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
		let player:Group | undefined = vm.playerVisualComponents.get(step.Request.SourceCharacterId)?.puppet;
		let enemyPuppet: Group | undefined = vm.enemyVisualComponents.get(step.Request.TargetCharacterId)?.puppet;
		let enemyHealth: HealthBar | undefined = vm.enemyVisualComponents.get(step.Request.TargetCharacterId)?.healthbar;
		
		if (player && enemyPuppet && enemyHealth)
		{
			this.leapAnim = new LeapAnimation({
				leapingNode: player,
				destination: enemyPuppet,
				leapDuration: 1,
				landingAnimation: new FunctionStep(() => {
					enemyHealth.changeHealth(step.GameState.EnemyCharacters.find((v, i, o) => v.Id == step.Request.TargetCharacterId)?.Health ?? 20)
				})
			})
		} else throw TypeError
	}
}