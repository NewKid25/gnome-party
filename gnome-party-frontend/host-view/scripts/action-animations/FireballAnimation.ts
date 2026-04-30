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
class FireballAnimation implements AnimationStep
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
		noteImg.src = "/img/Fireball.svg";
		let noteIcon = new Konva.Image({
			image: noteImg,
			scale: {x: vm.getCombatPuppetScale(), y: vm.getCombatPuppetScale()},
			width: 159,
			height: 73,
			opacity: 0.0,
		});

		if (player && targetPuppet)
		{

			let burns:AnimationStep[] = []
			console.log(step.Events);
			step.Events.filter((e) => e.event == "burn_status_applied").forEach((e) => {
				let burnTargetPuppet = vm.enemyVisualComponents.get(e.params.OwnerId)?.puppet;
				console.log(e);
				if (burnTargetPuppet)
				{
					burns.push(new TextPopupAnimation({
						text: "BURN",
						layer: vm.uiLayer,
						position: {
							x: burnTargetPuppet.position().x,
							y: burnTargetPuppet.position().y
						}
					}))
				}
			})

			burns.push(
				new FunctionStep(() => {
					step.GameState.EnemyCharacters.forEach((enemy) => {
						targetHealthBar = vm.enemyVisualComponents.get(enemy.Id)?.healthbar;
						newHealth = enemy.Health ?? 0;							
						targetHealthBar?.changeHealth(newHealth);
					})
				})
			)

			noteIcon.position(player.position());
			this.anim =	new SimultaneousAnimation([
				new ProjectileAnimation({
					node: noteIcon,
					startPos: {x: () => player.position().x, y: () => player.position().y - noteIcon.getHeight() * 0.3 / 2},
					endPos: {x: () => targetPuppet.position().x, y: () => targetPuppet.position().y - noteIcon.getHeight() * 0.3 / 2},
					layer: vm.uiLayer,
					duration: 0.6,
					impactAnimation: new SimultaneousAnimation(burns)
				}),
			])
		} else throw TypeError
	}
}