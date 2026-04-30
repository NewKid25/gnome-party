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
class DiscordAnimation implements AnimationStep
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
		let targetPuppet: Puppet | undefined;
		let targetHealthBar: HealthBar | undefined;
		let newHealth: number;

		targetPuppet = vm.enemyVisualComponents.get(step.Request.TargetCharacterId)?.puppet;
		targetHealthBar = vm.enemyVisualComponents.get(step.Request.TargetCharacterId)?.healthbar;
		newHealth = step.GameState.EnemyCharacters.find((v, i, o) => v.Id == step.Request.TargetCharacterId)?.Health ?? 0;

		

		const noteImg = new Image();
		noteImg.src = "/img/Discord.svg";
		let noteIcon = new Konva.Image({
			image: noteImg,
			scale: {x: vm.getCombatPuppetScale(), y: vm.getCombatPuppetScale()},
			width: 86,
			height: 91,
			opacity: 0.0,
		});

		if (player && targetPuppet)
		{
			noteIcon.position(player.position());
			this.anim =	new SimultaneousAnimation([
				new ProjectileAnimation({
					node: noteIcon,
					startPos: {x: () => player.position().x, y: () => player.position().y - noteIcon.getHeight() * 0.2 / 2},
					endPos: {x: () => targetPuppet.position().x, y: () => targetPuppet.position().y - noteIcon.getHeight() * 0.2 / 2},
					layer: vm.uiLayer,
					duration: 0.6,
					impactAnimation: new SimultaneousAnimation([
						new FunctionStep(() => {
							targetHealthBar?.changeHealth(newHealth);
						}),
						/*
						new TextPopupAnimation({
							text: "",
							position: targetPuppet.position(),
							layer: vm.uiLayer
						})
						*/
					]) 
				}),
			])
		} else throw TypeError
	}
}