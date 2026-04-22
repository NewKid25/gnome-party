import { Group } from "konva/lib/Group";
import FunctionStep from "../FunctionStep";
import GnomePuppet from "../puppets/GnomePuppet";
import HealthBar from "../HealthBar";
import AnimationStep from "../interfaces/AnimationStep";
import Puppet from "../interfaces/Puppet";
import { GameState, TurnStep } from "../interfaces/TurnStep";
import ViewManager from "../ViewManager";
import LeapAnimation from "../animations/LeapAnimation";
import { convertToObject } from "typescript";
import SimultaneousAnimation from "../SimultaneousAnimation";
import ProjectileAnimation from "../animations/ProjectileAnimation";
import Konva from "konva";
import TextPopupAnimation from "../animations/TextPopupAnimation";
import AnimationSequence from "../AnimationSequence";
import { Vector2d } from "konva/lib/types";


export default
class RottingAuraAnimation implements AnimationStep
{
	anim:AnimationStep
	play() 
	{
		this.anim.onFinish = this.onFinish
		this.anim.play();
	}
	onFinish: Function | undefined;

	constructor(step:TurnStep, vm:ViewManager) 
	{
		console.log(step.Request.SourceCharacterId, step.Request.TargetCharacterId)
		let targetPuppet: Puppet | undefined;

		targetPuppet = vm.playerVisualComponents.get(step.Request.TargetCharacterId)?.puppet;

		if (targetPuppet)
		{
			this.anim =	new SimultaneousAnimation([						
				new TextPopupAnimation({
					text: "VULNERABLE",
					position: targetPuppet.position(),
					layer: vm.uiLayer
				})				
			]) 
		} else throw TypeError
	}
}