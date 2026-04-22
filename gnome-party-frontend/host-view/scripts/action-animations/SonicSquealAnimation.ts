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
class SonicSquealAnimation implements AnimationStep
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
		let enemy:Puppet | undefined = vm.enemyVisualComponents.get(step.Request.SourceCharacterId)?.puppet;
		
		const roarImg = new Image();
		roarImg.src = "/img/Roar.svg";
		let roarIcon = new Konva.Image({
			image: roarImg,
			scale: {x: 0.75, y: 0.75},
			opacity: 0.0,
		});


		if (enemy)
		{
			this.anim = new AnimationSequence([
				new FunctionStep(()=> {
					roarIcon.position({x: enemy.position().x - enemy.width() * .95, y: enemy.position().y - roarIcon.getHeight() * 0.75 / 1.3 + 20});
				}),
				new SimultaneousAnimation([
					new SpawnIconAnimation({
						node: roarIcon,
						position: {x: () => enemy.position().x, y: () => enemy.position().y - roarIcon.getHeight() * 0.75 / 1.3},
						layer: vm.uiLayer,
					}),
					vm.updateAllHealth(step.GameState)
				])
			])	
		} else throw TypeError
	}
}