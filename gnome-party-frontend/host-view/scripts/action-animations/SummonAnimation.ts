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
import TweenFromCurrent from "../TweenFromCurrent";

export default
class SummonAnimation implements AnimationStep
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
		this.anim = new AnimationSequence();

		let slideSeq = new SimultaneousAnimation();

		let i = 0;
		vm.enemyVisualComponents.forEach((evc) => {
			let slideAnim = new TweenFromCurrent({
				node: evc.puppet,
				y: (i + 1) * vm.stage.height() / (step.GameState.EnemyCharacters.length + 1),
			})
			
			let hpSlideAnim = new TweenFromCurrent({
				node: evc.healthbar,
				y: (i + 1) * vm.stage.height() / (step.GameState.EnemyCharacters.length + 1) - evc.puppet.height() / 3.5,
			})

			i++;

			slideSeq.steps.push(slideAnim);
			slideSeq.steps.push(hpSlideAnim);
		});

		this.anim.steps.push(slideSeq);

		let enemyPuppet: Puppet;

		let allSummons = new AnimationSequence();

		step.Events.filter((e) => e.event == "summoned").forEach((e) => {
			let summonAnim:AnimationSequence = new AnimationSequence([
				new FunctionStep(() => {
					let type = e.params.SummonType;
					let puppet = vm.createEnemyPuppet(type);

					enemyPuppet = puppet;
								
					puppet.x(vm.stage.width() - 300);
					puppet.y((vm.enemyVisualComponents.size + 1) * vm.stage.height() / (step.GameState.EnemyCharacters.length + 1));
					
					console.log("i is ", (vm.enemyVisualComponents.size + 1));

					vm.mainLayer.add(puppet);
					// Create healthbar
					let enemy = step.GameState.EnemyCharacters.find((c) => c.Id == e.params.SummonId);
					if (enemy)
					{
						let healthbar:HealthBar = new HealthBar(enemy.MaxHealth, {x: 30, y: vm.HEALTHBAR_HEIGHT})
						healthbar.x(puppet.x() + puppet.width() /2 + 20);
						healthbar.y(puppet.y() - puppet.height() / 3.5);
			
						vm.uiLayer.add(healthbar);
			
						vm.enemyVisualComponents.set(enemy.Id, {puppet: puppet, healthbar: healthbar});

						console.log("Oh boy how I love ", e.params.SummonId);
					}
					else {
						console.log("Not found: ", e.params.SummonId);
					}

					puppet.opacity(0.0);
				}),

				new TweenFromCurrent({
					node: () => enemyPuppet,
					opacity: 1.0,
					duration: 0.7
				})
			])

			allSummons.steps.push(summonAnim);
		})
		
		this.anim.steps.push(allSummons);

	}
}