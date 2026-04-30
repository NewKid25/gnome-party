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
class RavenousGrowthAnimation implements AnimationStep
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
		
		const blockImg = new Image();
		blockImg.src = "/img/GreenAura.svg";
		let blockIcon = new Konva.Image({
			image: blockImg,
			scale: {x: vm.getCombatPuppetScale(), y: vm.getCombatPuppetScale()},
			width: 791,
			height: 190,
			opacity: 0.0,
		});


		if (enemy)
		{
			this.anim = new AnimationSequence([
				new FunctionStep(() => {
					blockIcon.position({x: enemy.position().x - enemy.width() * 4, y: enemy.position().y + enemy.height() * 0.5});
				}),
				new SimultaneousAnimation([
					new SpawnIconAnimation({
						node: blockIcon,
						position: {x: () => enemy.position().x, y: () => enemy.position().y + enemy.height() * 0.},
						layer: vm.uiLayer
					}),
					new TextPopupAnimation({
						text: "DMG UP",
						position: enemy.position(),
						layer: vm.uiLayer
					})
				])
			])	
		} else throw TypeError
	}
}