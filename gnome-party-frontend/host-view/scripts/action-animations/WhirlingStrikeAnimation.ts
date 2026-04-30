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
import AnimationSequence from "../AnimationSequence";
import TweenFromCurrent from "../TweenFromCurrent";

export default
class WhirlingStrikeAnimation implements AnimationStep
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
		let player:Puppet | undefined = vm.playerVisualComponents.get(step.Request.SourceCharacterId)?.puppet;
		
		this.anim = new AnimationSequence([])

		let i = 0;

		vm.enemyVisualComponents.forEach((evc, id) => {
			let enemyPuppet: Puppet | undefined = evc.puppet;
			let enemyHealth: HealthBar | undefined = evc.healthbar;
			
			console.log(player, enemyPuppet, enemyHealth);
	
			if (player && enemyPuppet && enemyHealth && step.GameState.EnemyCharacters.findIndex((e) => e.Id == id) > -1)
			{
				// let j = i;
				let leapAnim = new AnimationSequence([
					new TweenFromCurrent({
						node: player,
						x: () => enemyPuppet.position().x - enemyPuppet.width(),
						y: () => enemyPuppet.position().y,
						duration: 0.2
					}),
					new FunctionStep(() => {
						enemyHealth.changeHealth(step.GameState.EnemyCharacters.find((e) => e.Id == id)?.Health ?? 0)
					})
				]);

				this.anim.steps.push(leapAnim);

			}

			i++;
		})

		this.anim.steps.push( new TweenFromCurrent({
			node: player,
			x: player?.position().x,
			y: player?.position().y,
			duration: 0.3
		}))
	}
		
}