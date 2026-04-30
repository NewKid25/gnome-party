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
import SpawnIconAnimation from "../animations/SpawnIconAnimation";
import Konva from "konva";
import TextPopupAnimation from "../animations/TextPopupAnimation";
import AnimationSequence from "../AnimationSequence";
import { Vector2d } from "konva/lib/types";

export default
class BlockAnimation implements AnimationStep
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
		let player:Puppet | undefined = vm.playerVisualComponents.get(step.Request.SourceCharacterId)?.puppet;
		let allyPuppet: Puppet | undefined = vm.playerVisualComponents.get(step.Request.TargetCharacterId)?.puppet;
		
		const blockImg = new Image();
		blockImg.src = "/img/BlockIcon.svg";
		let blockIcon = new Konva.Image({
			image: blockImg,
			scale: {x: vm.getCombatPuppetScale(), y: vm.getCombatPuppetScale()},
			width: 172,
			height: 202,
			opacity: 0.0
		});

		let blockIconDest:Vector2d = {x: 0, y:0};

		if (player && allyPuppet)
		{
			this.anim = new AnimationSequence([ 
				new LeapAnimation({
					leapingNode: player,
					destination: allyPuppet,
					leapDuration: 1,
					landingAnimation: new AnimationSequence([
						new FunctionStep(() => {
							blockIcon.position(player.position());
							blockIconDest = player.position();
						}),
						new SimultaneousAnimation([
							new SpawnIconAnimation({
								node: blockIcon,
								position: {x: () => blockIconDest.x, y: () => blockIconDest.y - blockIcon.getHeight() * 0.2 / 2},
								layer: vm.uiLayer
							}),
							new TextPopupAnimation({
								text: "PROTECTED",
								position: allyPuppet.position(),
								layer: vm.uiLayer
							})
						])
					])
					
				}),
			])
		} else throw TypeError
	}
}