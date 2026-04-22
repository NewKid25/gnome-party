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
import TweenFromCurrent from "../TweenFromCurrent";
import AnimationPause from "../AnimationPause";


export default
class SoulDrainAnimation implements AnimationStep
{
	anim:AnimationSequence
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
		let sourceHealth:HealthBar | undefined = vm.enemyVisualComponents.get(step.Request.SourceCharacterId)?.healthbar;

		this.anim = new AnimationSequence();

		let simul:SimultaneousAnimation = new SimultaneousAnimation([]);

		step.Events.filter((e) => e.event == "damage").forEach((e) => {
			let targetPuppet: Puppet | undefined;
			let targetHealthBar: HealthBar | undefined;
			let newHealth: number;
	
			targetPuppet = vm.playerVisualComponents.get(e.params.TargetId)?.puppet;
			targetHealthBar = vm.playerVisualComponents.get(e.params.TargetId)?.healthbar;
			newHealth = step.GameState.PlayerCharacters.find((v, i, o) => v.Id == e.params.TargetId)?.Health ?? 0;
	
			const noteImg = new Image();
			noteImg.src = "/img/Soul Drain.svg";
			let noteIcon = new Konva.Image({
				image: noteImg,
				scale: {x: 0.2, y: 0.2},
				opacity: 0.0,
			});
	
			if (source && targetPuppet)
			{
				noteIcon.position(source.position());
				vm.uiLayer.add(noteIcon);
				simul.steps.push(
					new LeapAnimation({
						leapingNode: noteIcon,
						destination: targetPuppet,
						jumpHeight: 0,
						leapDuration: 1,
						landingAnimation: new FunctionStep(() => {
							targetHealthBar?.changeHealth(newHealth);
						}),
					}),
					new AnimationSequence([
						new TweenFromCurrent({
							node: noteIcon,
							opacity: 1.0,
							duration: 0.1
						}),
						new AnimationPause(800),
						new TweenFromCurrent({
							node: noteIcon,
							opacity: 0.0,
							duration: 0.1
						}),
					])
				);
			} else throw TypeError

		})

		this.anim.steps.push(simul);

		this.anim.steps.push(new FunctionStep(() => {
			sourceHealth?.changeHealth(step.GameState.EnemyCharacters.find((v, i, o) => v.Id == step.Request.SourceCharacterId)?.Health ?? 0);
		}))

	}
}