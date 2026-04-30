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
class CripplingShotAnimation implements AnimationStep
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
		let source:Puppet | undefined = vm.enemyVisualComponents.get(step.Request.SourceCharacterId)?.puppet;
		let targetPuppet: Puppet | undefined;
		let targetHealthBar: HealthBar | undefined;
		let newHealth: number;

		targetPuppet = vm.playerVisualComponents.get(step.Request.TargetCharacterId)?.puppet;
		targetHealthBar = vm.playerVisualComponents.get(step.Request.TargetCharacterId)?.healthbar;
		newHealth = step.GameState.PlayerCharacters.find((v, i, o) => v.Id == step.Request.TargetCharacterId)?.Health ?? 0;

		

		const noteImg = new Image();
		noteImg.src = "/img/Arrow.svg";
		let noteIcon = new Konva.Image({
			image: noteImg,
			scale: {x: vm.getCombatPuppetScale(), y: vm.getCombatPuppetScale()},
			width: 108,
			height: 29,
			opacity: 0.0,
		});

		if (source && targetPuppet)
		{
			noteIcon.position(source.position());
			this.anim =	new SimultaneousAnimation([
				new ProjectileAnimation({
					node: noteIcon,
					startPos: {x: () => source.position().x, y: () => source.position().y - noteIcon.getHeight() * 0.4 / 2},
					endPos: {x: () => targetPuppet.position().x, y: () => targetPuppet.position().y - noteIcon.getHeight() * 0.4 / 2},
					layer: vm.uiLayer,
					duration: 0.5,
					impactAnimation: new SimultaneousAnimation([
						new FunctionStep(() => {
							targetHealthBar?.changeHealth(newHealth);
						}),
						
						new TextPopupAnimation({
							text: "DMG DOWN",
							position: targetPuppet.position(),
							layer: vm.uiLayer
						})
						
					]) 
				}),
			])
		} else throw TypeError
	}
}